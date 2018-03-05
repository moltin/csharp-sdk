namespace Moltin.Models
{
    internal interface IMoltinCheckoutBindingModel
    {
        string Cart_id { get; set; }
        string Gateway { get; set; }
        string Shipping { get; set; }
        MoltinCustomerBindingModel Customer { get; set; }
    }

    internal interface ICheckoutBindingModel
    {
        string CartId { get; set; }
        string Gateway { get; set; }
        string Shipping { get; set; }
        CustomerBindingModel Customer { get; set; }
        AddressBindingModel ShipTo { get; set; }
        AddressBindingModel BillTo { get; set; }
    }

    internal interface ICustomerBindingModel
    {
        string Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
    }

    internal interface IAddressBindingModel
    {
        string Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Phone { get; set; }
        string SaveAs { get; set; }
        string Address1 { get; set; }
        string Address2 { get; set; }
        string City { get; set; }
        string County { get; set; }
        string PostCode { get; set; }
        string Country { get; set; }
    }
}