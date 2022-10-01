using System;
using XHRTools.requests;


namespace XHRTools
{
    public static class XhrRequest
    {
        
        public static void Learn(User user)
        {
            var bigLearnClient = new BigLearnClient();
            var latestSessionResult = bigLearnClient.PostRequest(new GetLatestSessionRequest());
            Console.WriteLine(latestSessionResult);
            var loginResult = bigLearnClient.PostRequest(new LoginRequest(user));
            Console.WriteLine(loginResult);
            var learnHitResult = bigLearnClient.PostRequest(new LearnHitRequest(latestSessionResult.SessionId));
            Console.WriteLine(learnHitResult);
        }
    }
}