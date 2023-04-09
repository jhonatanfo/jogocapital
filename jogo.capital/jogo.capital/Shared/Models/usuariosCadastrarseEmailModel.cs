using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
	public class usuariosCadastrarseEmailModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string email { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "A {0} precisa conter um tamanho de {2} e {1}", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string senha { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação")]
        [Compare("senha", ErrorMessage = "A Senha e a Confirmação Precisam Ser Iguais")]
        public string confirmarSenha { get; set; } = "";

        [Required]
        public string confirmarCodigo { get; set; } = "";

    }
}

