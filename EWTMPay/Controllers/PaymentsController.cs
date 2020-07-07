using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotAppData;
using BotAppData.Models;
using Microsoft.AspNetCore.Mvc;
using Yandex.Checkout.V3;

namespace EWTMPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly BotAppContext _context;

        public PaymentsController(BotAppContext context)
        {
            _context = context;
        }

        //// GET: api/Payments
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Payments>>> GetPayment()
        //{
        //    return await _context.Payment.ToListAsync();
        //}

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayments(Guid id)
        {
            var payments = await _context.Payment.FindAsync(id);

            if (payments == null)
            {
                return NotFound();
            }

            if (payments.IsPayed == false)
            {
                var client = new Client("591310", "test_1J9BQa-AGyxrN3U9x7CrJ6l4bM0ri8L5a5aGcBj7T_w").MakeAsync();

                if (payments.PaymentId == null)
                {
                    var url = Request;
                    var redirect = $"{url.Scheme}://{url.Host}/api/Payments/{id}";
                    var user = _context.Users.Where(user => user.ChatId == payments.ChatId).FirstOrDefault();
                    var data = await client.CreatePaymentAsync(
                        new NewPayment()
                        {
                            Amount = new Amount()
                            {
                                Value = payments.Amount,
                            },
                            Confirmation = new Confirmation()
                            {
                                Type = ConfirmationType.Redirect,
                                ReturnUrl = redirect
                            },
                            Description = "Order",
                            Receipt = new Receipt()
                            {
                                //Email = "blink.kerr@gmail.com",
                                Phone = user.Phone,
                                Items = new List<ReceiptItem>
                                {
                            {
                                new ReceiptItem
                                {
                                    Description = "Hello",
                                    Quantity = 1.0M,
                                    Amount = new Amount
                                    {
                                        Value = payments.Amount,
                                        Currency = "RUB"
                                    },
                                    VatCode = VatCode.NoVat,
                                    PaymentMode = PaymentMode.FullPayment,
                                    PaymentSubject = PaymentSubject.Service,

                                }
                            }
                                }
                            }
                        });
                    payments.PaymentId = data.Id;
                    await _context.SaveChangesAsync();
                    return Redirect(data.Confirmation.ConfirmationUrl);
                }
                else
                {
                    var payment = await client.GetPaymentAsync(payments.PaymentId);
                    if (payment.Status == PaymentStatus.WaitingForCapture)
                    {
                        var capt = await client.CapturePaymentAsync(payment);
                        if (capt.Status == PaymentStatus.Succeeded)
                        {
                            var sub = _context.Subscriptions.Find(payments.SubscriptionId);
                            var user = _context.Users.Where(user => user.ChatId == payments.ChatId).FirstOrDefault();
                            if (payments.IsExtends == true)
                            {
                                if(sub.Product == Products.Lessons || sub.Product == Products.Trial)
                                {
                                    sub.End = sub.End.AddMonths(1);
                                }
                                else if (sub.Product == Products.Maraphone)
                                {
                                    var group = _context.BroadcastGroup.Find(user.Group);
                                    if(group != null)
                                    {
                                        var lessons = _context.Lessons.Where(l => l.Group == group.Id).OrderByDescending(l => l.LessonAt).FirstOrDefault();
                                        if(lessons != null)
                                        {
                                            sub.End = lessons.LessonAt.AddHours(5);
                                        }
                                    }
                                }
                            }
                            sub.IsActive = true;
                            payments.IsPayed = true;
                            payment.CapturedAt = DateTime.Now;
                            await _context.SaveChangesAsync();
                            
                            if (user.Platform == 1)
                            {
                                return Redirect("https://langalgorithm.ru/Pages/Index/Telegram");
                            }
                            else if (user.Platform == 2)
                            {
                                return Redirect("https://langalgorithm.ru/Pages/Index/Vk");
                            }
                        }
                    }
                    else
                    {
                        return Redirect(payment.Confirmation.ConfirmationUrl);
                    }
                }
            }
            else
            {
                var user = _context.Users.Where(user => user.ChatId == payments.ChatId).FirstOrDefault();

                if (user.Platform == 1)
                {
                    return Redirect("https://langalgorithm.ru/Pages/Index/Telegram");
                }
                else if (user.Platform == 2)
                {
                    return Redirect("https://langalgorithm.ru/Pages/Index/Vk");
                }

            }

            return NotFound();
        }

        //// PUT: api/Payments/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for
        //// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutPayments(Guid id, Payments payments)
        //{
        //    if (id != payments.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(payments).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!PaymentsExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //// POST: api/Payments
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for
        //// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPost]
        //public async Task<ActionResult<Payments>> PostPayments(Payments payments)
        //{
        //    _context.Payment.Add(payments);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetPayments", new { id = payments.Id }, payments);
        //}

        //// DELETE: api/Payments/5
        //[HttpDelete("{id}")]
        //public async Task<ActionResult<Payments>> DeletePayments(Guid id)
        //{
        //    var payments = await _context.Payment.FindAsync(id);
        //    if (payments == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Payment.Remove(payments);
        //    await _context.SaveChangesAsync();

        //    return payments;
        //}

        //private bool PaymentsExists(Guid id)
        //{
        //    return _context.Payment.Any(e => e.Id == id);
        //}
    }
}
