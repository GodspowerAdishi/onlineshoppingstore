using OnlineShoppingStore.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
               
namespace OnlineShoppingStore.Controllers
{
    public class PaymentController : Controller
    {
        // Work with Paypal payment
        private Payment payment;

        // Create a payment using an APIContext
        private Payment CreatePayment(APIContext apicontext, string redirectURl)
        {

            var ItemLIst = new ItemList() { items = new List<Item>() };

            if (Session["cart"] != "")
            {
                List<Models.Home.Item> cart = (List<Models.Home.Item>)(Session["cart"]);
                foreach (var item in cart)
                {
                    ItemLIst.items.Add(new Item()
                    {
                        name = item.Product.ProductName.ToString(),
                        currency = "TK",
                        price = item.Product.Price.ToString(),
                        quantity = item.Product.Quantity.ToString(),
                        sku = "sku"
                    });
                }

                var payer = new Payer() { payment_method = "paypal" };

                // Do the configuration RedirectURLs here with redirectURLs object
                var redirUrl = new RedirectUrls()
                {
                    cancel_url = redirectURl + "&Cancel=true",
                    return_url = redirectURl
                };

                // Create details object
                var details = new Details()
                {
                    tax = "1",
                    shipping = "1",
                    subtotal = "1"
                };

                // Create amount object
                var amount = new Amount()
                {
                    currency = "USD",

                    total = Session["SesTotal"].ToString(),
                    details = details
                };

                // Create transaction 
                var transactionList = new List<Transaction>();
                transactionList.Add(new Transaction()
                {
                    description = "Transaction Description",
                    invoice_number = Convert.ToString((new Random()).Next(100000)),
                    amount = amount,
                    item_list = ItemLIst

                });

                this.payment = new Payment()
                {
                    intent = "sale",
                    payer = payer,
                    transactions = transactionList,
                    redirect_urls = redirUrl
                };
            }

            return this.payment.Create(apicontext);

        }

        // Create Execute Payment Method
        private object ExecutePayment(APIContext apicontext, string payerId, string PaymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = PaymentId };
            return this.payment.Execute(apicontext, paymentExecution);
        }


        // GET: Payment (Create payment with Paypal method)
        public ActionResult PaymentWithPaypal()
        {
            
            // Getting context from the paypal based on clientId and clientsecret for payment
            APIContext apicontext = PaypalConfiguration.GetAPIContext();
            try
            {

                string PayerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(PayerId) && PayerId != null)
                {
                    string baseURi = Request.Url.Scheme + "://" + Request.Url.Authority +
                                     "/Payment/PaymentWithPaypal?";
                    
                    var Guid = Convert.ToString((new Random()).Next(100000000));
                    var createPayment = this.CreatePayment(apicontext, baseURi + "guid=" + Guid);

                    // Get links returned from paypal response to create call function
                    var links = createPayment.links.GetEnumerator();
                    string paypalRedirectURL = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;

                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectURL = lnk.href;
                        }

                    }
                        Session.Add(Guid, createPayment.id);
                        return Redirect(paypalRedirectURL);

                    }
             
                else
                {
                    // This one will be executed when we have received all the payment params from previous call
                    var guid = Request.Params["guid"];
                    var executedPaymnt = ExecutePayment(apicontext, PayerId, Session[guid] as string);


                    if (executedPaymnt.ToString().ToLower() != "approved")
                    {
                        return View("FailureView");
                    }

                }
            }
            catch (Exception ex)
            {
                PaypalLogger.Log("Error: " + ex.Message);
                return View("FailureView");

            }
            return View("SuccessView");

        }
    }
}
