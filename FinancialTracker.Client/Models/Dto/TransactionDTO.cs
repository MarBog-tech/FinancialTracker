﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FinancialTracker.Client.Models.Dto;

public class TransactionDTO
{
    public Guid Id { get; set; }
    [Required]
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    [Required]
    public bool IsIncome { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
}