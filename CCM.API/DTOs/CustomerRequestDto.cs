namespace CCM.API.DTOs;
public class CustomerRequestDto {
    public string CustomerName{get;set;}=""; public string CustomerType{get;set;}="";
    public string ContactPerson{get;set;}=""; public string Email{get;set;}="";
    public string? Phone{get;set;} public string? GSTIN{get;set;} public string? PAN{get;set;}
    public string? PaymentTerms{get;set;} public decimal? CreditLimit{get;set;}
    public string? Industry{get;set;} public string BillingAddress{get;set;}=""; public string? Remarks{get;set;}
}