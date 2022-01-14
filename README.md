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

TODO Clean up the consumer code and add documentaiton for public api

# How to publish:

```shell
dotnet nuget push bin/Debug/Harjis.Outbox.1.0.0.nupkg --api-key <apikeyhere> --source https://api.nuget.org/v3/index.json
```