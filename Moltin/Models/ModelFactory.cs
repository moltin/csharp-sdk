namespace Moltin.Models
{
    /// <summary>
    /// Factory classes for factorizing models into moltin models
    /// </summary>
    internal class ModelFactory
    {

        /// <summary>
        /// Creates the moltin specific checkout model
        /// </summary>
        /// <param name="model">The default c# request model</param>
        /// <param name="output">The output model</param>
        /// <returns></returns>
        public IMoltinCheckoutBindingModel Create(ICheckoutBindingModel model)
        {

            // Get the billing and shipping ids
            var billingId = model.BillTo.Id;
            var shippingId = model.ShipTo.Id;          

            // If we have both shipping and billing
            if (!string.IsNullOrEmpty(billingId) && !string.IsNullOrEmpty(shippingId))
            {

                // Return our model with both addresses
                return new MoltinCheckoutBothAddressesBindingModel
                {
                    Cart_id = model.CartId,
                    Gateway = model.Gateway,
                    Shipping = model.Shipping,

                    Customer = this.Create(model.Customer),
                    Ship_to = model.ShipTo.Id,
                    Bill_to = model.BillTo.Id
                };
            }

            // If we only have the billing id
            if (!string.IsNullOrEmpty(billingId) && string.IsNullOrEmpty(shippingId))
            {

                // Return our model with the billing address
                return new MoltinCheckoutBillingAddressOnlyBindingModel
                {
                    Cart_id = model.CartId,
                    Gateway = model.Gateway,
                    Shipping = model.Shipping,

                    Customer = this.Create(model.Customer),
                    Ship_to = this.Create(model.ShipTo),
                    Bill_to = model.BillTo.Id
                };
            }

            // If we only have the shipping id
            if (string.IsNullOrEmpty(billingId) && !string.IsNullOrEmpty(shippingId))
            {

                // Return our model with the shipping address
                return new MoltinCheckoutShippingAddressOnlyBindingModel
                {
                    Cart_id = model.CartId,
                    Gateway = model.Gateway,
                    Shipping = model.Shipping,

                    Customer = this.Create(model.Customer),
                    Ship_to = model.ShipTo.Id,
                    Bill_to = this.Create(model.BillTo)
                };
            }

            // Return the moltin model
            return new MoltinCheckoutBindingModel
            {
                Cart_id = model.CartId,
                Gateway = model.Gateway,
                Shipping = model.Shipping,

                Customer = this.Create(model.Customer),
                Ship_to = this.Create(model.ShipTo),
                Bill_to = this.Create(model.BillTo)
            };
        }

        /// <summary>
        /// Creates the moltin specific customer model
        /// </summary>
        /// <param name="model">The default c# request model</param>
        /// <returns></returns>
        public MoltinCustomerBindingModel Create(ICustomerBindingModel model)
        {

            // Return the moltin model
            return new MoltinCustomerBindingModel
            {
                Id = model.Id,
                First_name = model.FirstName,
                Last_name = model.LastName,
                Email = model.Email
            };
        }

        /// <summary>
        /// Creates the moltin specific address model
        /// </summary>
        /// <param name="model">The default c# request model</param>
        /// <returns></returns>
        public MoltinAddressBindingModel Create(IAddressBindingModel model)
        {

            // Return the moltin model
            return new MoltinAddressBindingModel
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
}
