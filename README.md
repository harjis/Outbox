# Producer

How to use:
```c#
// The domain object that is going to be sent as event payload
// Models/Post.cs 
public class Post
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }
}

// Domain event
// Events/PostCreated.cs

using Outbox.Producer.Events;
public class PostCreated : IOutboxEvent<Post>
{
    public string Id { get; }
    public string AggregateType { get; set; }
    public string AggregateId { get; set; }
    public string Type { get; set; }
    public Post Payload { get; set; }

    public PostCreated(Post post)
    {
        Id = Guid.NewGuid().ToString();
        AggregateType = "Post";
        AggregateId = post.Id.ToString();
        Type = "PostCreated";
        Payload = post;
    }

    public Outbox.Producer.Models.Outbox ToOutboxModel()
    {
        return new()
        {
            id = Id,
            aggregatetype = AggregateType,
            aggregateid = AggregateId,
            type = Type,
            payload = JsonSerializer.Serialize(Payload)
        };
    }
}

//Services/PostService.cs
public class PostService
{
    private readonly UnitOfWork _unitOfWork;

    public PostService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AddPost(Post post)
    {
        await _unitOfWork.ExecuteInTransaction(async () =>
        {
            await _unitOfWork.PostRepository.Add(post);
            await _unitOfWork.Save();
            // Call EventManager.FireEvent() in the same transaction where Domain Object is persisted
            // so that they either succeed or fail together.
            await _unitOfWork.EventManager.FireEvent(new PostCreated(post));
        });
    }
}
```

# Consumer

```c#
public class PostsConsumer
{
    private readonly string _topic;
    private readonly IConsumer<string, string> _kafkaConsumer;
    
    private readonly ConsumedEventRepository<CommentDbContext, Post> _consumedEventRepository;
    private readonly PostRepository _postRepository;

    public PostsConsumer(ConsumedEventRepository<CommentDbContext, Post> consumedEventRepository, PostRepository postRepository)
    {
        _consumedEventRepository = consumedEventRepository;
        _postRepository = postRepository;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "ConsumerGroupId",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _topic = "Post.events";
        _kafkaConsumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    public void StartConsumerLoop(CancellationToken cancellationToken)
    {
        _kafkaConsumer.Subscribe(_topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var receivedEvent = ProcessOutboxEvent(_kafkaConsumer.Consume(cancellationToken));
                if (!_consumedEventRepository.HasBeenConsumed(receivedEvent))
                {
                    _consumedEventRepository.Add(receivedEvent);
                    // Do something with the ReceivedEvent
                }
                else
                {
                    Console.WriteLine(
                        $"Service received message {receivedEvent.Id} which has already been consumed");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"Consume error: {e.Error.Reason} was fatal: {e.Error.IsFatal}");

                if (e.Error.IsFatal)
                {
                    Console.WriteLine("Fatal error: killing the background process");
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
                break;
            }
        }
    }

    public void Dispose()
    {
        _kafkaConsumer.Close();
        _kafkaConsumer.Dispose();
    }
    
    private ReceivedEvent<Post> ProcessOutboxEvent(ConsumeResult<string, string> consumeResult)
    {
        var messageIdBuffer = consumeResult.Message.Headers.GetLastBytes("id");
        var id = System.Text.Encoding.UTF8.GetString(messageIdBuffer, 0, messageIdBuffer.Length);
        
        var eventTypeBuffer = consumeResult.Message.Headers.GetLastBytes("eventType");
        var type = System.Text.Encoding.UTF8.GetString(eventTypeBuffer, 0, eventTypeBuffer.Length);

        return new ReceivedEvent<Post>(id, type, JsonSerializer.Deserialize<Post>(consumeResult.Message.Value));
    }
}
```

# How to publish:

```shell
dotnet nuget push bin/Debug/Harjis.Outbox.1.0.0.nupkg --api-key <apikeyhere> --source https://api.nuget.org/v3/index.json
```