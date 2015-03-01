using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using BendigoBank.DataVisualisation;

namespace BendigoBank
{
    // NOTE: A better implementation of this system may be a static class which logs in each time
    // a function is called. It's unclear at this point!
 
    // NOTE: This project is being developed in an exploratory way. As such, certain strings and values
    // will be hardcoded, code may be repeated, etc.
    // I like to get things working and only once things are working, begin examining the code for 
    // potential refactorings and tidy-ups. I find this methodology to be fast and intrinsically motivating.
    // However, I am not suggesting that this methodology is appropriate for ALL projects.
 
    /// <summary>
    /// A class which represents a Bendigo Bank online banking session.
    /// </summary>
    public class OnlineBankingSession
    {
        private const string ERROR_HEADER = "[OnlineBankingSession]";
        private const string ACCOUNTS_URL = "https://banking.bendigobank.com.au/banking/accounts/";
        private const string MOVE_MONEY_URL = "https://banking.bendigobank.com.au/banking/move_money/"; //transfer, bpay, pay_anyone 
        private const string CONTACTS_URL = "https://banking.bendigobank.com.au/banking/contacts/";
        private const string MESSAGES_URL = "https://banking.bendigobank.com.au/banking/messages/";

        private CookieContainer Cookies; // This could be declared/initialised within the HttpClientHandler constructor,
                                         // but isn't at the moment for the sake of debugability.

        private HttpClient HttpClient;   // For sending and receiving messages to/from Bendigo Bank.

        private string CsrfToken;   // This token is sent back and forth between the http client and server for security.

        /// <summary>
        /// Creates a new OnlineBankingSession.
        /// </summary>
        public OnlineBankingSession()
        {
            Cookies = new CookieContainer();
            HttpClient = new HttpClient(
            new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = true,
                CookieContainer = Cookies
            });
            HttpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest"); // These are necessary for many CSRF-vulnerable HTTP requests.
            CsrfToken = string.Empty;
        }

        /// <summary>
        /// Checks whether the session is authenticated.
        /// </summary>
        /// <returns>True if an authentic session is alive.</returns>
        public async Task<bool> IsLoggedInAsync()
        {
            HttpResponseMessage r = await HttpClient.GetAsync(CONTACTS_URL);
            return r.Headers.CacheControl.MustRevalidate;
        }

        // TODO: Debug - 500 Internal Server Error
        /*
        public async Task<bool> IsBpayContactValid(string billerCode, string referenceNumber)
        {
            if (await IsLoggedInAsync())
            {
                
                //Go to contacts page
                var newResponse = await HttpClient.GetAsync("https://banking.bendigobank.com.au/banking/contacts/bpay/new");
                
                //Check biller code
                var req2 = new HttpRequestMessage(HttpMethod.Get, "https://banking.bendigobank.com.au/banking/bpay_billers/" + billerCode);
                req2.Headers.Referrer = new Uri("https://banking.bendigobank.com.au/banking/contacts/bpay/new");
                req2.Headers.Add("X-CSRF-Token", CsrfToken);
                var codeResponse = await HttpClient.SendAsync(req2);

                // egbdfgh

                var reqTwo = WebRequest.Create("https://banking.bendigobank.com.au/banking/bpay_billers/" + billerCode);

                //Check reference number
                var req3 = new HttpRequestMessage(HttpMethod.Post, string.Format("https://banking.bendigobank.com.au/banking/bpay_billers/{0}/validate_crn", billerCode));
                req3.Headers.Add("X-CSRF-Token", CsrfToken);
                req3.Headers.Add("X-Requested-With", "XMLHttpRequest");
                req3.Content = new StringContent(string.Format("customer_reference_number={0}", referenceNumber), Encoding.UTF8);
                req3.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
                var validateResponse = await HttpClient.SendAsync(req3);

                return validateResponse.StatusCode == HttpStatusCode.NoContent;
            }
            return false;
        }
         */

        /// <summary>
        /// Retrieves bank account information for the currently logged in user.
        /// </summary>
        /// <param name="getLineChartData">If true, line chart data is collected for each BankAccount object.</param>
        /// <returns>A collection of BankAccount objects for each of the user's bank accounts.</returns>
        public async Task<BankAccountCollection> GetBankAccountsAsync(bool getLineChartData)
        {
            BankAccountCollection accounts = new BankAccountCollection();
            if (await IsLoggedInAsync())
            {
                // Get the user's accounts page HTML.
                HttpResponseMessage accountsResponse = await HttpClient.GetAsync(ACCOUNTS_URL); 
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(await accountsResponse.Content.ReadAsStringAsync());
                // Extract the account groups from the HTML (account groups are delineated by holder name e.g. JOHN A SMITH)
                var groupNodes = GetNodesWithAttribute(document.DocumentNode, "li", "class", "grouped-list__group");
                foreach (var groupNode in groupNodes)
                {
                    // Extract the name of the holder for the current group.
                    string holder = groupNode.Descendants("h6").First().InnerText.Trim().Replace("&amp;", "&");
                    // Extract all accounts from the current group.
                    var accountNodes = GetNodesWithAttribute(groupNode, "div", "class", "account panel ");
                    foreach (var accountNode in accountNodes)
                    {
                        // Extract BankAccount member data from select elements and attributes within the account element.
                        string name = GetNodesWithAttribute(accountNode, "span", "class",
                            "overflow-ellipsis account-name-and-number__name h4").First().InnerText.Trim()
                            .Replace(".", string.Empty).Replace("&amp;", "&");
                        string id = accountNode.Id.Substring(8);

                        var numbersNode = GetNodesWithAttribute(
                            accountNode, "span", "class", "account-name-and-number__numbers h6").First();
                        string number = numbersNode.Descendants("span").Where(
                            n => !n.Attributes.Contains("class")).Last().InnerText.Trim();
                        string bsb = numbersNode.Descendants("span").Where(
                            n => !n.Attributes.Contains("class")).First().InnerText.Trim();

                        float available = float.Parse(accountNode.Attributes["data-available-balance"].Value);
                        var currentNode = numbersNode.Descendants("dt").FirstOrDefault(
                            n => n.Attributes.Contains("class") && n.Attributes["class"].Value ==
                            "key-value-list__value");
                        float current = currentNode != null ?
                            float.Parse(currentNode.InnerText.Trim().Replace("$", string.Empty)) :
                            available;

                        if (getLineChartData)
                        {
                            HttpResponseMessage transactionsResponse = await HttpClient.GetAsync(
                                string.Format("{0}{1}/transactions/chart.json?limit=19&available_balance={2}",
                                ACCOUNTS_URL, id, available));
                            string responseContent = await transactionsResponse.Content.ReadAsStringAsync();
                            accounts.Add(new BankAccount(holder, id, name, bsb, number, current, available,
                                JsonConvert.DeserializeObject<BankTransactionCollection>(responseContent)));
                        }
                        accounts.Add(new BankAccount(holder, id, name, bsb, number, current, available));
                    }
                }
            }
            else
                System.Diagnostics.Debug.WriteLine("GetBankAccountsAsync() - Not logged in.", ERROR_HEADER);
            return accounts;
        }

        /// <summary>
        /// Retrieves contact information for the currently logged in user.
        /// </summary>
        /// <returns>A Collection of BankContact objects for each contact registered by the user.</returns>
        public async Task<BankContactCollection> GetContactsAsync()
        {
            BankContactCollection contacts = new BankContactCollection();
            if (await IsLoggedInAsync())
            {
                // Get the user's contacts page HTML.
                HttpResponseMessage contactsResponse = await HttpClient.GetAsync(CONTACTS_URL);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(await contactsResponse.Content.ReadAsStringAsync());
                // Extract the contact container elements.
                var contactNodes = GetNodesWithAttribute(document.DocumentNode, "div", "class", "panel panel--badged grouped-list__items__item ");
                foreach (var contactNode in contactNodes)
                {
                    // Extract BankContact member data from select elements and attributes within the account element.
                    string id = contactNode.Id.Split('-').Last();

                    // Extract the header and avatar container elements.
                    var headerNode = GetNodesWithAttribute(contactNode, "div", "class", "flag").First();
                    var avatarNode = GetNodesWithAttribute(headerNode, string.Empty, "class", "avatar--").First();
                    // The avatar implementation for the current contact may be concrete or placeholder.
                    object avatar = null;
                    if (avatarNode.Attributes.Contains("class")
                        && avatarNode.Attributes["class"].Value.Contains("logo"))
                        avatar = avatarNode.Attributes["src"].Value;
                    else if (avatarNode.Attributes.Contains("style"))
                        avatar = new PlaceholderAvatar
                        {
                            Initials = avatarNode.InnerText.Trim(),
                            Colour = avatarNode.Attributes["style"].Value.Split(':').Last()
                        };

                    // Extract the name container element.
                    var nameNode = GetNodesWithAttribute(headerNode, "h4", "data-semantic", "contact-name").FirstOrDefault();
                    // A name may not be specified in the name container element.
                    string name;
                    if (nameNode != null)
                        name = nameNode.InnerText.Trim();
                    else
                        name = avatarNode.Attributes["title"].Value;

                    // Extract the element containing payment history information.
                    var lastPaidNode = GetNodesWithAttribute(headerNode, "h6", "class", "h6").FirstOrDefault();
                    string lastPaid = string.Empty;
                    if (lastPaidNode != null)
                        lastPaid = lastPaidNode.InnerText.Trim();

                    // Extract the elements containing contact details.
                    var detailNodes = GetNodesWithAttribute(contactNode, "span", "class", "uilist__item__detail");
                    // Regardless of type, contacts will always have two detail parts.
                    string detailOne = detailNodes.ElementAt(0).InnerText.Trim();
                    string detailTwo = detailNodes.ElementAt(1).InnerText.Trim();
                    // There are only two types of contact, BPAY Payee and PayAnyone.
                    if (contactNode.Id.Contains("bpay"))
                        contacts.Add(new BpayBankContact(id, avatar, name, lastPaid, detailOne, detailTwo));
                    else
                        contacts.Add(new PayAnyoneBankContact(id, avatar, name, lastPaid, detailOne, detailTwo));
                }
            }
            return contacts;
        }

        /// <summary>
        /// Updates the user's registered email address.
        /// </summary>
        /// <param name="newEmail">The new email address to be set.</param>
        /// <returns>True if the change was successful.</returns>
        public async Task<bool> UpdateEmailAddressAsync(string newEmail)
        {
            if (await IsLoggedInAsync())
            {
                if (!string.IsNullOrWhiteSpace(newEmail))
                {
                    StringContent query = new StringContent(string.Format("utf8=%E2%9C%93&_method=patch&authenticity_token={0}&customer_updater%5Bemail_address%5D={1}&button=",
                        CsrfToken, newEmail.Trim()), Encoding.UTF8);
                    HttpResponseMessage r = await HttpClient.PostAsync("https://banking.bendigobank.com.au/banking/customer", query);
                    if (r.Headers.Location != null && r.Headers.Location.AbsoluteUri == "https://banking.bendigobank.com.au/banking/settings")
                        return true;
                    else
                        System.Diagnostics.Debug.WriteLine("UpdateEmailAddressAsync() - Unknown error.", ERROR_HEADER);
                }
                else
                    System.Diagnostics.Debug.WriteLine("UpdateEmailAddressAsync() - Bad email address.", ERROR_HEADER);
            }
            else
                System.Diagnostics.Debug.WriteLine("UpdateEmailAddressAsync() - Not logged in.", ERROR_HEADER);
            return false;
        }

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="oldPassword">The user's current password.</param>
        /// <param name="newPassword">The new password to be set.</param>
        /// <param name="newPasswordAgain">The new password repeated for accuracy.</param>
        /// <returns></returns>
        public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword, string newPasswordAgain)
        {
            if (await IsLoggedInAsync())
            {
                if (!string.IsNullOrWhiteSpace(oldPassword) && !string.IsNullOrWhiteSpace(newPassword) && !string.IsNullOrWhiteSpace(newPasswordAgain))
                {
                    if (newPassword == newPasswordAgain)
                    {
                        // The maximum length allowed for a password is 8 characters.
                        TruncatePassword(ref oldPassword);
                        TruncatePassword(ref newPassword);
                        TruncatePassword(ref newPasswordAgain);
                        StringContent query = new StringContent(string.Format("start=&old={0}&new={1}&new2={2}",
                            oldPassword, newPassword, newPasswordAgain), Encoding.UTF8);
                        HttpResponseMessage r = await HttpClient.PostAsync(
                            "https://banking.bendigobank.com.au/pkmspasswd.form", query);
                        if ((await r.Content.ReadAsStringAsync()).Contains("Moved Temporarily"))
                            return true;
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("ChangePasswordAsync() - New passwords do not match.", ERROR_HEADER);
                }
                else
                    System.Diagnostics.Debug.WriteLine("ChangePasswordAsync() - Invalid function parameters", ERROR_HEADER);
            }
            else
                System.Diagnostics.Debug.WriteLine("ChangePasswordAsync() - Not logged in.", ERROR_HEADER);
            return false;
        }

        /// <summary>
        /// Creates an authenticated Bendigo Bank online banking session.
        /// </summary>
        /// <param name="accessId">The access ID used to log in.</param>
        /// <param name="password">The password corresponding to the access ID.</param>
        /// <param name="authToken">A 6-digit authentication token.</param>
        /// <returns></returns>
        public async Task<bool> LogInAsync(string accessId, string password, string authToken)
        {
            if (!await IsLoggedInAsync())
            {
                if (!string.IsNullOrWhiteSpace(accessId) && !string.IsNullOrWhiteSpace(password))
                {
                    TruncatePassword(ref password); // The maximum length allowed for a password is 8 characters.
                    StringContent query = new StringContent(string.Format(
                        "start=&password={1}%C2%B6{2}&login-form-type=pwd&username={0}&passwordx={1}&authenticationKey={2}",
                    accessId, password, authToken), Encoding.UTF8);
                    // Attempt login.
                    HttpResponseMessage loginResponse = await HttpClient.PostAsync("https://banking.bendigobank.com.au/pkmslogin.form", query);

                    if (loginResponse.Headers.Location != null) // ... if the response is a redirect ...
                    {
                        // ... follow the redirection! 
                        HttpResponseMessage sessionResponse = await HttpClient.GetAsync(loginResponse.Headers.Location);
                        if (sessionResponse.StatusCode == HttpStatusCode.Found) // ... and if we get another redirection ...
                        {
                            // ... follow that one too!
                            HttpResponseMessage accountsResponse = await HttpClient.GetAsync(sessionResponse.Headers.Location);
                            if (accountsResponse.IsSuccessStatusCode) // ... finally, if we reach the accounts page ...
                            {
                                // Parse the CSRF synchroniser token.
                                HtmlDocument doc = new HtmlDocument();
                                doc.LoadHtml(await accountsResponse.Content.ReadAsStringAsync());
                                CsrfToken = doc.DocumentNode.Descendants("meta").Where(m => m.Attributes.Contains("name")
                                    && m.Attributes["name"].Value == "csrf-token").First().Attributes["content"].Value;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Creates an authenticated Bendigo Bank online banking session.
        /// </summary>
        /// <param name="accessId">The access ID used to log in.</param>
        /// <param name="password">The password corresponding to the access ID.</param>
        /// <returns></returns>
        public async Task<bool> LogInAsync(string accessId, string password)
        {
            if (!await IsLoggedInAsync())
            {
                if (!string.IsNullOrWhiteSpace(accessId) && !string.IsNullOrWhiteSpace(password))
                {
                    TruncatePassword(ref password); // The maximum length allowed for a password is 8 characters.
                    StringContent query = new StringContent(string.Format(
                        "start=&password={1}%C2%B6{2}&login-form-type=pwd&username={0}&passwordx={1}&authenticationKey=",
                    accessId, password, string.Empty), Encoding.UTF8);
                    // Attempt login.
                    HttpResponseMessage loginResponse = await HttpClient.PostAsync("https://banking.bendigobank.com.au/pkmslogin.form", query);

                    if (loginResponse.Headers.Location != null) // ... if the response is a redirect ...
                    {
                        // ... follow the redirection! 
                        HttpResponseMessage sessionResponse = await HttpClient.GetAsync(loginResponse.Headers.Location);
                        if (sessionResponse.StatusCode == HttpStatusCode.Found) // ... and if we get another redirection ...
                        {
                            // ... follow that one too!
                            HttpResponseMessage accountsResponse = await HttpClient.GetAsync(sessionResponse.Headers.Location);
                            if (accountsResponse.IsSuccessStatusCode) // ... finally, if we reach the accounts page ...
                            {
                                // Parse the CSRF synchroniser token.
                                HtmlDocument doc = new HtmlDocument();
                                doc.LoadHtml(await accountsResponse.Content.ReadAsStringAsync());
                                CsrfToken = doc.DocumentNode.Descendants("meta").Where(m => m.Attributes.Contains("name")
                                    && m.Attributes["name"].Value == "csrf-token").First().Attributes["content"].Value;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// A function for transferring money from one account to another (within the user's accessible accounts).
        /// </summary>
        /// <param name="sender">A BankAccount object for the sending account.</param>
        /// <param name="recipient">A BankAccount object for receiving account.</param>
        /// <param name="transferData">An object which holds details for an account transfer.</param>
        /// <returns></returns>
        public async Task<bool> AccountTransferAsync(BankAccount sender, BankAccount recipient, BankTransfer transferData)
        {
            if (await IsLoggedInAsync())
            {
                if (sender.Available >= transferData.Amount)
                {
                    StringContent query = new StringContent(
                        string.Format(
                                "utf8=%E2%9C%93&authenticity_token={0}&transfer%5Bfrom_id%5D={1}&transfer%5Bto_id%5D={2}&transfer%5Bamount%5D={3}&transfer%5Bdescription%5D={4}&transfer%5Bfuture_payment_date%5D={5}",
                                CsrfToken, sender.Id, recipient.Id, transferData.Amount.ToString(), transferData.Description, transferData.PaymentDate), Encoding.UTF8);
                    HttpResponseMessage r = await HttpClient.PostAsync(MOVE_MONEY_URL + "transfer", query);
                    if ((await r.Content.ReadAsStringAsync()).Contains("redirected_from_payment_confirm=true"))
                        return true;
                }
            }
            return false;
        }

        #region helper functions
        private IEnumerable<HtmlNode> GetNodesWithAttribute(HtmlNode parent, string nodeType, string attrName, string attrValue)
        {
            if (!string.IsNullOrWhiteSpace(nodeType))
                return parent.Descendants(nodeType).Where(n => n.Attributes.Contains(attrName)
                    && n.Attributes[attrName].Value.Contains(attrValue));
            else
                return parent.Descendants().Where(n => n.Attributes.Contains(attrName)
                    && n.Attributes[attrName].Value.Contains(attrValue));
        }

        private static void TruncatePassword(ref string password)
        {
            password = password.Length > 8 ? password.Substring(0, 8) : password;
        }
        #endregion

        // URLS AND OTHER STUFF FOR FUTURE IMPLEMENTATION
        /*
        private string qsb_contact_bpay_add = "utf8=%E2%9C%93&authenticity_token={0}%3D&move_money=&entity%5Bbiller_code%5D={1}&entity%5Bdescription%5D={2}&entity%5Bcustomer_reference_number%5D={3}";
        private string qsb_contact_bpay_update = "utf8=%E2%9C%93&_method=patch&authenticity_token={0}%3D&entity%5Bdescription%5D={1}&entity%5Bcustomer_reference_number%5D={2}";
        private string qsb_contact_payanyone_add = "utf8=%E2%9C%93&authenticity_token={0}%3D&entity%5Baccount_title%5D={1}&entity%5Bbsb%5D={2}&entity%5Baccount_number%5D={3}&entity%5Btoken%5D={4}";
        private string qsb_contact_payanyone_update = "utf8=%E2%9C%93&_method=patch&authenticity_token={0}%3D&entity%5Baccount_title%5D={1}&entity%5Baccount_number%5D={2}&entity%5Btoken%5D={3}";
        private string url_post_contact_add = "https://banking.bendigobank.com.au/banking/contacts/{0}"; //bpay, pay_anyone
        private string url_post_contact_update = "https://banking.bendigobank.com.au/banking/contacts/{0}/{1}"; //bpay, pay_anyone ~ id
        private string qsb_bpay_payment = "utf8=%E2%9C%93&authenticity_token={0}%3D&bpay%5Bto_id%5D=&bpay%5Bpayee_attributes%5D%5Bbiller_code%5D={1}&bpay%5Bcustomer_reference_number%5D={2}&bpay%5Bfrom_id%5D={3}&bpay%5Bamount%5D={4}&bpay%5Bdescription%5D={5}&bpay%5Bfuture_payment_date%5D={6}&bpay%5Btoken%5D={7}";
        private string qsb_payanyone_payment = "utf8=%E2%9C%93&authenticity_token={0}%3D&pay_anyone%5Bto_id%5D=&pay_anyone%5Bpayee_attributes%5D%5Bname%5D={1}&pay_anyone%5Bpayee_attributes%5D%5Bbsb%5D={2}&pay_anyone%5Bpayee_attributes%5D%5Baccount_number%5D={3}&pay_anyone%5Bfrom_id%5D={4}&pay_anyone%5Bamount%5D={5}&pay_anyone%5Bdescription%5D={6}&pay_anyone%5Bfuture_payment_date%5D={7}&pay_anyone%5Btoken%5D={8}";
        private string qsb_message_new = "utf8=%E2%9C%93&authenticity_token={0}%3D&message%5Bsubject%5D={1}&message%5Bbody%5D={2}";
        private string qsb_message_reply = "utf8=%E2%9C%93&authenticity_token={0}%3D&message%5Bbody%5D={1}";
        private string qsb_generic_delete = "_method=delete&authenticity_token={0}%3D";

        private string url_favourite_payments = "https://banking.bendigobank.com.au/banking/favourite_payments"; 
        private string qsb_favourite_transaction = "payment_id={0}&payment_type={1}";

        private string url_post_message_reply = "https://banking.bendigobank.com.au/banking/messages/{0}/replies";
        private string url_post_scheduled_delete = "https://banking.bendigobank.com.au/banking/payments/{0}/{1}"; //bpay, pay_anyone, transfer ~ id

        private string url_get_bpay_biller_validate = ;
        private string url_get_payanyone_bsb_validate = "https://banking.bendigobank.com.au/banking/branches/{0}";
        private string url_get_payanyone_limit_validate = "https://banking.bendigobank.com.au/banking/pay_anyone_daily_limit/today";
         
        TODO:
        addition/deletion of contacts
        get collections of transactions
        payanyone/bpay payments
        message sending/reading
         */
    }
}
