using System.ComponentModel.DataAnnotations;

namespace JevApplicationTeste.Api.Models;

public sealed record ClassificarRequest(
    [property: Required, StringLength(5000, MinimumLength = 10)] string Descricao);

public sealed record Decisao(string Opcao, double Confianca, Dictionary<string, double> Probabilidades);
public sealed record ClassificarResponse(string Modo, string Modelo, Decisao Categoria,
    Decisao Prioridade, string EquipeSugerida, bool RequerRevisaoHumana);

// Nomes dos campos correspondem ao contrato HTTP da TypeSafe (serialização camelCase).
public sealed record ChoiceQuestion(string Type, string Instructions, Dictionary<string, string> Criteria);
public sealed record JevRequest(string Model, object State, Dictionary<string, ChoiceQuestion> Questions);
public sealed record JevAnswer(string Type, string Choice, double? Confidence,
    Dictionary<string, double>? Probabilities);
public sealed record JevResponse(string Model, Dictionary<string, JevAnswer>? Answers);
