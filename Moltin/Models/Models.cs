using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Moltin.Models
{
    public class CheckoutBindingModel : ICheckoutBindingModel
    {
        [Required] public string CartId { get; set; }
        [Required] public string Gateway { get; set; }
        [Required] public string Shipping { get; set; }
        [Required] public CustomerBindingModel Customer { get; set; }
        [Required] public AddressBindingModel ShipTo { get; set; }
        [Required] public AddressBindingModel BillTo { get; set; }
    }

    public class CustomerBindingModel : ICustomerBindingModel
    {
        public string Id { get; set; }

        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string Email { get; set; }
    }

    public class AddressBindingModel : IAddressBindingModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address2 { get; set; }
        public string SaveAs { get; set; }

        [Required] public string Address1 { get; set; }
        [Required] public string City { get; set; }
        [Required] public string County { get; set; }
        [Required] public string PostCode { get; set; }
        [Required] public string Country { get; set; }
    }

    [JsonConverter(typeof(CartModifierSerializer))]
    public class MoltinModifierModel
    {
        public MoltinModifierModel()
        {
            Values = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Values { get; set; }
    }

    public class CartModifierSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var modifier = value as MoltinModifierModel;
            writer.WriteStartObject();
            foreach (var pair in modifier.Values)
            {
                writer.WritePropertyName(pair.Key);
                writer.WriteValue(pair.Value);
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();
            return new MoltinModifierModel
            {
                Values = properties.ToDictionary(x => x.Name, x => x.Value.ToString())
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(MoltinModifierModel).IsAssignableFrom(objectType);
        }
    }

    #region Private

    internal class MoltinCheckoutBothAddressesBindingModel : IMoltinCheckoutBindingModel
    {
        public string Cart_id { get; set; }
        public string Gateway { get; set; }
        public string Shipping { get; set; }
        public MoltinCustomerBindingModel Customer { get; set; }
        public string Ship_to { get; set; }
        public string Bill_to { get; set; }
    }

    internal class MoltinCheckoutShippingAddressOnlyBindingModel : IMoltinCheckoutBindingModel
    {
        public string Cart_id { get; set; }
        public string Gateway { get; set; }
        public string Shipping { get; set; }
        public MoltinCustomerBindingModel Customer { get; set; }
        public string Ship_to { get; set; }
        public MoltinAddressBindingModel Bill_to { get; set; }
    }

    internal class MoltinCheckoutBillingAddressOnlyBindingModel : IMoltinCheckoutBindingModel
    {
        public string Cart_id { get; set; }
        public string Gateway { get; set; }
        public string Shipping { get; set; }
        public MoltinCustomerBindingModel Customer { get; set; }
        public MoltinAddressBindingModel Ship_to { get; set; }
        public string Bill_to { get; set; }
    }

    internal class MoltinCheckoutBindingModel : IMoltinCheckoutBindingModel
    {
        public string Cart_id { get; set; }
        public string Gateway { get; set; }
        public string Shipping { get; set; }
        public MoltinCustomerBindingModel Customer { get; set; }
        public MoltinAddressBindingModel Ship_to { get; set; }
        public MoltinAddressBindingModel Bill_to { get; set; }
    }

    internal class MoltinCustomerBindingModel
    {
        public string Id { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Email { get; set; }
    }

    internal class MoltinAddressBindingModel
    {
        public string Id { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Phone { get; set; }
        public string Save_as { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
    }

    #endregion  
}
