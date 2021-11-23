using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using FunctionAppDistancias.Models;

namespace FunctionAppDistancias;

public static class ConversorDistancias
{
    [FunctionName(nameof(ConversorDistancias))]
    [OpenApiOperation(operationId: nameof(ConversorDistancias), tags: new[] { "Distancias" })]
    [OpenApiParameter(name: "milhas", In = ParameterLocation.Query, Required = true, Type = typeof(double), Description = "Valor em Milhas a ser convertido")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Distancia), Description = "Resultado da conversao de Milhas para Km")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(FalhaConversao), Description = "Falha na conversão de uma distancia em Milhas")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = null)] HttpRequest req, ILogger log)
    {
        var valorAConverter = req.Query["milhas"];
        log.LogInformation($"Distancia recebida para conversao: {valorAConverter}");

        double? milhas = null;
        try
        {
            if (!String.IsNullOrWhiteSpace(valorAConverter))
                milhas = JsonSerializer.Deserialize<double>(valorAConverter);
        }
        catch
        {
            log.LogError("Erro durante a conversao da distancia em milhas!");
        }

        if (milhas is null || milhas.Value <= 0.0)
        {
            var mensagem =
                $"A distancia informada ({valorAConverter}) deve ser um valor numerico maior do que zero!";
            log.LogError(mensagem);
            return new BadRequestObjectResult(
                new FalhaConversao() { Mensagem = mensagem });
        }

        var resultado = new Distancia(milhas.Value);
        log.LogInformation($"{milhas} milhas = {resultado.Km} Km");
        return new OkObjectResult(resultado);
    }
}