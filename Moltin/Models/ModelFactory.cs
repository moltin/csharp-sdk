namespace Moltin.Models
{
    /// <summary>
    ///     Factory classes for factorizing models into moltin models
    /// </summary>
    internal static class ModelFactory
    {
        /// <summary>
        ///     Creates the moltin specific checkout model
        /// </summary>
        /// <param name="model">The default c# request model</param>
        /// <param name="output">The output model</param>
        /// <returns></returns>
        public static IMoltinCheckoutBindingModel Create(ICheckoutBindingModel model)
        {
            // Get the billing and shipping ids
            var billingId = model.BillTo.Id;
            var shippingId = model.ShipTo.Id;

            // If we have both shipping and billing
            if (!string.IsNullOrEmpty(billingId) && !string.IsNullOrEmpty(shippingId))
                return new MoltinCheckoutBothAddressesBindingModel
                {
                    Cart_id = model.CartId,
                    Gateway = model.Gateway,
                    Shipping = model.Shipping,

                    Customer = Create(model.Customer),
                    Ship_to = model.ShipTo.Id,
                    Bill_to = model.BillTo.Id
                };

            // If we only have the billing id
            if (!string.IsNullOrEmpty(billingId) && string.IsNullOrEmpty(shippingId))
                return new MoltinCheckoutBillingAddressOnlyBindingModel
                {
                    Cart_id = model.CartId,
                    Gateway = model.Gateway,
                    Shipping = model.Shipping,

                    Customer = Create(model.Customer),
                    Ship_to = Create(model.ShipTo),
                    Bill_to = model.BillTo.Id
                };

            // If we only have the shipping id
            if (string.IsNullOrEmpty(billingId) && !string.IsNullOrEmpty(shippingId))
                return new MoltinCheckoutShippingAddressOnlyBindingModel
                {
                    Cart_id = model.CartId,
                    Gateway = model.Gateway,
                    Shipping = model.Shipping,

                    Customer = Create(model.Customer),
                    Ship_to = model.ShipTo.Id,
                    Bill_to = Create(model.BillTo)
                };

            // Return the moltin model
            return new MoltinCheckoutBindingModel
            {
                Cart_id = model.CartId,
                Gateway = model.Gateway,
                Shipping = model.Shipping,

                Customer = Create(model.Customer),
                Ship_to = Create(model.ShipTo),
                Bill_to = Create(model.BillTo)
            };
        }

        /// <summary>
        ///     Creates the moltin specific customer model
        /// </summary>
        /// <param name="model">The default c# request model</param>
        /// <returns></returns>
        public static MoltinCustomerBindingModel Create(ICustomerBindingModel model) => new MoltinCustomerBindingModel
        {
            Id = model.Id,
            First_name = model.FirstName,
            Last_name = model.LastName,
            Email = model.Email
        };

        /// <summary>
        ///     Creates the moltin specific address model
        /// </summary>
        /// <param name="model">The default c# request model</param>
        /// <returns></returns>
        public static MoltinAddressBindingModel Create(IAddressBindingModel model) => new MoltinAddressBindingModel
        {
            Id = model.Id,
            First_name = model.FirstName,
            Last_name = model.LastName,
            Phone = model.Phone,

            Save_as = model.SaveAs,
            Address_1 = model.Address1,
            Address_2 = model.Address2,
            City = model.City,
            County = model.County,
            Country = model.Country,
            Postcode = model.PostCode
        };
    }
}