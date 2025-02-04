using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Extensions
{
    public static class ModelStateExtension
    {
        // Criando um método de extensão para ModelStateDictionary através do "this" para extrair todos os erros de validação 
        // e os retorna em uma lista de strings, facilitando a padronização de respostas da API com o ResultViewModel
        public static List<string> GetErros(this ModelStateDictionary modelState)
        {
            var result = new List<string>();
            foreach (var item in modelState.Values)
            {
                result.AddRange(item.Errors.Select(error => error.ErrorMessage));
            }
            return result;
        }
    }
}