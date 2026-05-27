using Microsoft.AspNetCore.Http.HttpResults;

namespace MiniJiraAspire.Server.Chatbot;

public static class ChatbotEndpoints
{
    public static void MapChatbotEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/chat", AskChatbot)
            .WithName("AskChatbot")
            .WithTags("Chatbot")
            .WithSummary("Ask the Mini Jira chatbot a question");
    }

    private static async Task<Results<Ok<ChatResponse>, BadRequest<ChatResponse>, ProblemHttpResult>> AskChatbot(
        ChatRequest request,
        IMiniJiraKnowledgeProvider knowledgeProvider,
        ILmStudioChatClient lmStudioChatClient,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return TypedResults.BadRequest(new ChatResponse("Please enter a question."));
        }

        try
        {
            var knowledge = await knowledgeProvider.GetKnowledgeAsync(cancellationToken);
            var answer = await lmStudioChatClient.AskAsync(knowledge, request.Message.Trim(), cancellationToken);

            return TypedResults.Ok(new ChatResponse(answer));
        }
        catch (HttpRequestException)
        {
            return TypedResults.Problem(
                "The chatbot could not reach LM Studio. Make sure LM Studio is running with the local server enabled.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
