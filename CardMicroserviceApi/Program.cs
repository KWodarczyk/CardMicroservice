using CardMicroserviceApi.CardDataApiClient;
using CardMicroserviceApi.Services;
using System.Text.Json.Serialization;

namespace CardMicroserviceApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpClient("CardDataApi");
            builder.Services.AddSingleton<CardDataServiceClient>(sp =>
            {
                string baseUrl = builder.Configuration["CardDataApi:BaseUrl"]
                    ?? throw new InvalidOperationException("CardDataApi:BaseUrl configuration is missing.");
                HttpClient httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("CardDataApi");
                return new CardDataServiceClient(baseUrl, httpClient);
            });

            // Both ICardService implementations are registered by their concrete type so the
            // factory below can switch between them based on "CardDataApi:UseMock" without
            // either implementation depending on the other.
            builder.Services.AddSingleton<CardServiceMock>();
            builder.Services.AddSingleton<CardServiceClient>();
            builder.Services.AddSingleton<ICardService>(sp =>
            {
                bool useMock = builder.Configuration.GetValue<bool>("CardDataApi:UseMock");
                return useMock
                    ? sp.GetRequiredService<CardServiceMock>()
                    : sp.GetRequiredService<CardServiceClient>();
            });
            builder.Services.AddSingleton<ICardActionsCalculator, CardActionsCalculator>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
