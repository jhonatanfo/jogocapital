using System;

namespace jogo.capital.Shared.Models
{
	public class resultModel
	{
		public bool Sucesso { get; set; }
		public IEnumerable<string>? Erros { get; set; }
		public string? Token { get; set; }

        public static explicit operator resultModel(HttpResponseMessage v)
        {
            throw new NotImplementedException();
        }
    }
}

