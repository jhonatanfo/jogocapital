﻿using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
	public class enviarSmsModel
    {

        [Required]
        [Display(Name = "Destinatário")]
        public string Destinatario { get; set; } = "";

        [Required]
        [Display(Name = "Texto")]
        public string Texto { get; set; } = "";

    }
}

