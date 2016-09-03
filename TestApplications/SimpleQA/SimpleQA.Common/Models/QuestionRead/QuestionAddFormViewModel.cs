﻿using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    [MetadataType(typeof(IQuestionData))]
    public class QuestionAddFormViewModel : IModel, IQuestionData
    {
        public String Title { get; set; }
        public String Content { get; set; }
        public String[] Tags { get; set; }
    }
}