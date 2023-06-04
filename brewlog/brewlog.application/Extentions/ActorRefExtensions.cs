using Akka.Actor;
using System.Runtime.CompilerServices;

namespace brewlog.application.Extentions
{
    //public static class ActorRefExtensions
    //{
    //    public static async Task<T?> AskWithError<T>(
    //        this IActorRef actor,
    //        object message) where T : class
    //    {
    //        var response = await actor.Ask<ActorResponseData>(message);

    //        if (!response.Successful)
    //            throw new Exception(response.ErrorMessage);

    //        return response.As<T>();
    //    }

    //    public void RespondWith(IActorRef to, object message)
    //    {
    //        to.Tell(new ActorResponseData(message, null));
    //    }
    //}

    //public record ActorResponseData(object? Response, string? ErrorMessage)
    //{
    //    public bool Successful => string.IsNullOrEmpty(ErrorMessage);

    //    public T? As<T>() where T : class
    //    {
    //        return Response as T;
    //    }
    //}
}
