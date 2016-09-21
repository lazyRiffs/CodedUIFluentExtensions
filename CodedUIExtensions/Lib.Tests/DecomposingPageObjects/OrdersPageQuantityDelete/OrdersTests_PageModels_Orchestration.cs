using System;
using System.Collections.Generic;
using System.Linq;
using CaptainPav.Testing.UI.CodedUI;
using CaptainPav.Testing.UI.CodedUI.Html;
using CaptainPav.Testing.UI.CodedUI.PageModeling;
using CaptainPav.Testing.UI.CodedUI.PageModeling.Html;
using CaptainPav.Testing.UI.CodedUI.PageModeling.Html.ControlWrappers;
using CaptainPav.Testing.UI.PageModeling;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lib.Tests.DecomposingPageObjects.OrdersPageQuantityDelete.Orchestration
{
    public interface ILayoutPageModel : IPageModel
    {
        INavPageModel Nav { get; }
    }

    // demo: these tests do not yet need the Login or Account Settings pages
    // so we're just returning something that provides navigation
    // away from the page
    public interface INavPageModel : IPageModel
    {
        IClickablePageModel<IRegisterPageModel> Register { get; }
        IClickablePageModel<ILayoutPageModel> Login { get; }
        IClickablePageModel<IOrdersPageModel> Orders { get; }
        IClickablePageModel<ILayoutPageModel> AccountSettings { get; }     
    }

    public interface IOrdersPageModel : ILayoutPageModel
    {
        IClickablePageModel<IOrdersPageModel> AddButton { get; }
        IClickablePageModel<IOrdersPageModel> CancelButton { get; }
        
        IReadWriteValuePageModel<string, IOrdersPageModel> OrderId { get; }   
        IReadWriteValuePageModel<string, IOrdersPageModel> Name { get; }

        IReadWriteTextValuePageModel<int?, IOrdersPageModel> Quantity { get; }
        IReadWriteTextValuePageModel<decimal?, IOrdersPageModel> Price { get; }

        IClickablePageModel<IOrdersPageModel> Save { get; }
        
        // demo: no need for methods like FindOrderById(string orderId)
        IEnumerable<IOrdersRowPageModel> OrdersList { get; } 
    }

    // demo: why not inherit ILayoutPageModel?
    public interface IOrdersRowPageModel : IPageModel
    {
        IValuedPageModel<string> OrderId { get; }
        IValuedPageModel<string> Name { get; }
        
        ITextValuedPageModel<int> Quantity { get; }
        ITextValuedPageModel<decimal> Price { get; } 
        
        IClickablePageModel<IOrdersPageModel> Remove { get; }  
    }

    // this could be from the business layer
    public interface IOrderItem
    {
        string OrderId { get; }
        string Name { get; }
        int Quantity { get; }
        decimal Price { get; }
    }

    public interface IOrdersOrchestrations
    {
        IOrdersPageModel CreateOrder(string orderId, string name, int quantity, decimal price);
        IOrdersPageModel CreateOrder(IOrderItem order);
    }

    public interface IRegisterPageModel : ILayoutPageModel
    {
        IReadWriteValuePageModel<string, IRegisterPageModel> Username { get; }
        IValueablePageModel<string, IRegisterPageModel> Password { get; } 
        IValueablePageModel<string, IRegisterPageModel> ConfirmPassword { get; } 
        IClickablePageModel<ILayoutPageModel> Register { get; } // goes to account settings, but for brievity 
    }

    public interface IRegisterOrchestrations
    {
        ILayoutPageModel Register(string username, string password);
    }

    [CodedUITest]
    public class OrdersTests_PageModels
    {
        protected static readonly Random rand = new Random(98765);

        protected BrowserWindow window;
        protected string newUsername;
        protected string newPassword;
        //protected OrdersPageObject ordersPageObject;
        protected IOrdersPageModel ordersPageModel;

        [TestInitialize]
        public void GivenNewUser()
        {
            window = BrowserWindow.Launch($"{TestConfig.UrlBase}/DecomposingPageObjects/Change5");

            this.newUsername = Guid.NewGuid().ToString("N");
            this.newPassword = "pass";

            /****************
             * HtmlCustom nav = new HtmlCustom(window);
             * nav.SearchProperties.Add(HtmlControl.PropertyNames.TagName, "nav");
             * 
             * HtmlButton registerButton = new HtmlButton(nav);
             * registerButton.SearchProperties.Add(HtmlButton.PropertyNames.DisplayText, "Register");
             * 
             * Mouse.Click(registerButton);
             * 
             * RegistrationControlPageObject registrationControl = new RegistrationControlPageObject(window);
             * 
             * registrationControl.SetFormValues(this.newUsername, this.newPassword, this.newPassword);
             * registrationControl.ClickRegister();
             * 
             * HtmlButton ordersButton = new HtmlButton(nav);
             * ordersButton.SearchProperties.Add(HtmlButton.PropertyNames.DisplayText, "Orders");
             * 
             * Mouse.Click(ordersButton);
             * 
             * this.ordersPageModel = new OrdersPageModel(window);
             */

            /*******************
             * // we don't really care which page we start on
             * // we'll just use the navigation to get where we need to go
             * var startinPageModel = new LayoutPageModelBase(this.window);
             * 
             * // demo: isn't this much nicer?
             * this.ordersPageModel = 
             * startinPageModel.Nav
             *                 .Register.Click()
             *                 .Username.SetValue(this.newUsername)
             *                 .Password.SetValue(this.newPassword)
             *                 .ConfirmPassword.SetValue(this.newPassword)
             *                 .Register.Click()
             *                 .Nav
             *                 .Orders.Click();
             */

            var registerOrchestrations = new RegisterOrchestations(
                new LayoutPageModelBase(this.window).Nav
                                                    .Register.Click());

            // demo: one high level method call
            // demo: what happens if registration changes?
            this.ordersPageModel =
            registerOrchestrations.Register(this.newUsername, this.newPassword)
                                  .Nav
                                  .Orders.Click();
        }

        [TestMethod]
        public void NewOrderFieldsVisibleWhenAddClickAndCancelHidesForm()
        {
            Assert.IsTrue(this.ordersPageModel
                              .AddButton.IsRendered());
            Assert.IsFalse(this.ordersPageModel
                               .CancelButton.IsRendered());

            Assert.IsFalse(this.ordersPageModel
                               .OrderId.IsRendered());
            Assert.IsFalse(this.ordersPageModel
                               .Name.IsRendered());
            Assert.IsFalse(this.ordersPageModel
                               .Price.IsRendered());

            this.ordersPageModel.AddButton.Click();

            Assert.IsTrue(this.ordersPageModel.AddButton.IsNotRendered());
            Assert.IsTrue(this.ordersPageModel.CancelButton.IsRendered());

            Assert.IsTrue(this.ordersPageModel.OrderId.IsRendered());
            Assert.IsTrue(this.ordersPageModel.Name.IsRendered());
            Assert.IsTrue(this.ordersPageModel.Price.IsRendered());

            this.ordersPageModel.CancelButton.Click();

            // demo: duplicate assertion block
            Assert.IsTrue(this.ordersPageModel.AddButton.IsRendered());
            Assert.IsFalse(this.ordersPageModel.CancelButton.IsRendered());

            Assert.IsFalse(this.ordersPageModel.OrderId.IsRendered());
            Assert.IsFalse(this.ordersPageModel.Name.IsRendered());
            Assert.IsFalse(this.ordersPageModel.Price.IsRendered());
        }

        [TestMethod]
        public void SaveButtonEnabledOnlyWhenFormComplete()
        {
            this.ordersPageModel.AddButton.Click();

            // demo: need to set orderId to show quantity to blank it out
            // demo: should this be an orchestration?
            this.ordersPageModel
                .OrderId.SetValue("abc")
                .Name.SetValue("")
                .Quantity.SetValueText("")
                .Price.SetValueText("")
                .OrderId.SetValue("");

            Assert.IsFalse(this.ordersPageModel.Save.IsActionable());

            // demo: can't orchestrate since we are asserting between steps
            this.ordersPageModel
                .OrderId.SetValue(Guid.NewGuid().ToString("N"));

            Assert.IsFalse(this.ordersPageModel.Save.IsActionable());

            this.ordersPageModel
                .Price.SetValue((decimal)rand.NextDouble() * 100);

            Assert.IsFalse(this.ordersPageModel.Save.IsActionable());

            this.ordersPageModel
                .Quantity.SetValue(rand.Next(1, 100));

            Assert.IsFalse(this.ordersPageModel.Save.IsActionable());

            this.ordersPageModel
                .Name.SetValue(Guid.NewGuid().ToString("N"));

            Assert.IsTrue(this.ordersPageModel.Save.IsActionable());
        }

        [TestMethod]
        public void AfterSavingOrder_OrderAppearsInList()
        {
            string newOrderId = Guid.NewGuid().ToString("N");
            string newName = Guid.NewGuid().ToString("N");
            int newQuantity = rand.Next(1, 100);
            decimal newPrice = 12.1m;

            /*******************
             * this.ordersPageModel.AddButton.Click();
             * 
             * this.ordersPageModel
             *     .OrderId.SetValue(newOrderId)
             *     .Name.SetValue(newName)
             *     .Quantity.SetValue(newQuantity)
             *     .Price.SetValue(newPrice);
             * 
             * var orders = this.ordersPageModel.Save.Click();
             */

            // demo: no need for details, just do it
            var orders = new OrdersOrchestrations(this.ordersPageModel).CreateOrder(newOrderId, newName, newQuantity, newPrice);

            var match = orders.OrdersList
                              .FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(newOrderId, x.OrderId.Value));

            Assert.IsNotNull(match);
            Assert.IsTrue(StringComparer.Ordinal.Equals(newOrderId, match.OrderId.Value));
            Assert.IsTrue(StringComparer.Ordinal.Equals(newName, match.Name.Value));
            Assert.IsTrue(newPrice == match.Price.Value);
            Assert.IsTrue(newQuantity == match.Quantity.Value);
        }
    }

    public class LayoutPageModelBase : HtmlPageModelBase<HtmlDiv>, ILayoutPageModel
    {
        public LayoutPageModelBase(BrowserWindow bw) : base(bw)
        {
        }

        protected HtmlDiv LayoutBodyDiv => this.parent.Find<HtmlDiv>("layoutBodyContainer");

        protected override HtmlDiv Me => this.LayoutBodyDiv;

        // demo: Me gets overridden by derived types so need to capture
        // the layout body separately
        public INavPageModel Nav => new NavPageModel(this.parent, this.LayoutBodyDiv.Find<HtmlNavigation>());
    }

    public class NavPageModel : HtmlChildPageModelBase<HtmlNavigation>, INavPageModel
    {
        public NavPageModel(BrowserWindow bw, HtmlNavigation me) : base(bw, me)
        {
        }

        public IClickablePageModel<IRegisterPageModel> Register
            => this.Me
                   .Find<HtmlButton>(HtmlButton.PropertyNames.InnerText, "Register", PropertyExpressionOperator.EqualTo)
                   .AsPageModel(new RegisterPageModel(this.parent));

        public IClickablePageModel<ILayoutPageModel> Login
            => this.Me
                   .Find<HtmlButton>(HtmlButton.PropertyNames.InnerText, "Login", PropertyExpressionOperator.EqualTo)
                   .AsPageModel(new LayoutPageModelBase(this.parent));

        public IClickablePageModel<IOrdersPageModel> Orders
            => this.Me
                   .Find<HtmlButton>(HtmlButton.PropertyNames.InnerText, "Orders", PropertyExpressionOperator.EqualTo)
                   .AsPageModel(new OrdersPageModel(this.parent));

        public IClickablePageModel<ILayoutPageModel> AccountSettings
            => this.Me
                   .Find<HtmlButton>(HtmlButton.PropertyNames.InnerText, "Account Settings", PropertyExpressionOperator.EqualTo)
                   .AsPageModel(new LayoutPageModelBase(this.parent));
    }

    public class OrdersPageModel : HtmlPageModelBase<HtmlDiv>, IOrdersPageModel
    {
        public OrdersPageModel(BrowserWindow bw) : base(bw)
        {
        }

        /********************
         * HtmlDiv ordersDiv = new HtmlDiv(this.window);
         * ordersDiv.SearchProperties.Add(HtmlDiv.PropertyNames.Id, "ordersControl", PropertyExpressionOperator.EqualTo);

         * return ordersDiv;
         */
        
        // demo: fluent syntax
        protected override HtmlDiv Me => this.parent.Find<HtmlDiv>("ordersControl");

        /***************
         *  HtmlButton addButton = new HtmlButton(this.OrdersDiv);
         *  addButton.SearchProperties.Add(HtmlButton.PropertyNames.InnerText, "Add", PropertyExpressionOperator.EqualTo);
         *
         *  return addButton;
         */

        // demo: fluent syntax
        // demo: consistent strategy for controls
        // AsPageModel extensions provide consistent access to test
        // controls for their state and perform common actions like setting text
        public IClickablePageModel<IOrdersPageModel> AddButton 
            => this.Me
                   .Find<HtmlButton>(HtmlButton.PropertyNames.InnerText, "Add", PropertyExpressionOperator.EqualTo)
                   .AsPageModel(this);

        public IClickablePageModel<IOrdersPageModel> CancelButton
            => this.Me
                   .Find<HtmlButton>(HtmlButton.PropertyNames.InnerText, "Cancel", PropertyExpressionOperator.EqualTo)
                   .AsPageModel(this);

        public IReadWriteValuePageModel<string, IOrdersPageModel> OrderId
            => this.Me
                   .Find<HtmlEdit>(HtmlEdit.PropertyNames.ControlDefinition, "orderId", PropertyExpressionOperator.Contains)
                   .AsPageModel(this);

        public IReadWriteValuePageModel<string, IOrdersPageModel> Name
            => this.Me
                   .Find<HtmlEdit>(HtmlEdit.PropertyNames.ControlDefinition, "name", PropertyExpressionOperator.Contains)
                   .AsPageModel(this);

        public IReadWriteTextValuePageModel<int?, IOrdersPageModel> Quantity
            => this.Me
                   .Find<HtmlEdit>(HtmlEdit.PropertyNames.ControlDefinition, "quantity", PropertyExpressionOperator.Contains)
                   .AsPageModel(this, s => { int q;
                                               if (!int.TryParse(s, out q)) return null;
                                               return (int?)q;
                   }, i => i.HasValue ? i.Value.ToString("D") : "");

        public IReadWriteTextValuePageModel<decimal?, IOrdersPageModel> Price
            => this.Me
                   .Find<HtmlEdit>(HtmlEdit.PropertyNames.ControlDefinition, "price", PropertyExpressionOperator.Contains)
                   .AsPageModel(this, s => {
                       decimal q;
                       if (!decimal.TryParse(s, out q)) return null;
                       return (decimal?)q;
                   }, i => i.HasValue ? i.Value.ToString("N2") : "");

        public IClickablePageModel<IOrdersPageModel> Save
            => this.Me
                   .Find<HtmlButton>(HtmlButton.PropertyNames.DisplayText, "Save", PropertyExpressionOperator.EqualTo)
                   .AsPageModel(this);

        public IEnumerable<IOrdersRowPageModel> OrdersList
            => this.Me
                   .FindAll<HtmlReadonlyListItem>()
                   .Select(x => new OrdersRowPageModel(this.parent, x, this));

        public INavPageModel Nav => new NavPageModel(this.parent, this.Me.Find<HtmlNavigation>());
    }

    public class OrdersRowPageModel : HtmlChildPageModelBase<HtmlReadonlyListItem>, IOrdersRowPageModel
    {
        protected readonly IOrdersPageModel ordersPageParent;

        public OrdersRowPageModel(BrowserWindow bw, HtmlReadonlyListItem me, IOrdersPageModel ordersPageModel) : base(bw, me)
        {
            this.ordersPageParent = ordersPageModel;
        }

        public IValuedPageModel<string> OrderId 
            => this.Me
                   .Find<HtmlSpan>(HtmlSpan.PropertyNames.ControlDefinition, "orderId", PropertyExpressionOperator.Contains)
                   .AsValuedPageModel(x => x.DisplayText);

        public IValuedPageModel<string> Name
             => this.Me
                    .Find<HtmlSpan>(HtmlSpan.PropertyNames.ControlDefinition, "name", PropertyExpressionOperator.Contains)
                    .AsValuedPageModel(x => x.DisplayText);

        public ITextValuedPageModel<int> Quantity
             => this.Me
                    .Find<HtmlSpan>(HtmlSpan.PropertyNames.ControlDefinition, "quantity", PropertyExpressionOperator.Contains)
                    .AsTextValuedPageModel(int.Parse);

        public ITextValuedPageModel<decimal> Price
             => this.Me
                    .Find<HtmlSpan>(HtmlSpan.PropertyNames.ControlDefinition, "price", PropertyExpressionOperator.Contains)
                    .AsTextValuedPageModel(decimal.Parse);

        public IClickablePageModel<IOrdersPageModel> Remove
            => this.Me
                   .Find<HtmlButton>(HtmlButton.PropertyNames.InnerText, "Delete", PropertyExpressionOperator.EqualTo)
                   .AsPageModel(this.ordersPageParent);
    }

    public class OrdersOrchestrations : IOrdersOrchestrations
    {
        public IOrdersPageModel Model { get; }
        public OrdersOrchestrations(IOrdersPageModel model)
        {
            this.Model = model;
        }

        public IOrdersPageModel CreateOrder(string orderId, string name, int quantity, decimal price)
        {
            if (this.Model.AddButton.IsRendered())
            {
                this.Model.AddButton.Click();
            }

            return
            this.Model
                .OrderId.SetValue(orderId)
                .Quantity.SetValue(quantity)
                .Price.SetValue(price)
                .Name.SetValue(name)
                .Save.Click();
        }

        public IOrdersPageModel CreateOrder(IOrderItem order)
        {
            return this.CreateOrder(order.OrderId, order.Name, order.Quantity, order.Price);
        }
    }

    public class RegisterPageModel :  HtmlPageModelBase<HtmlDiv>, IRegisterPageModel
    {
        public RegisterPageModel(BrowserWindow bw) : base(bw)
        {
        }

        protected override HtmlDiv Me => this.parent.Find<HtmlDiv>("registerControl");

        public INavPageModel Nav => new NavPageModel(this.parent, this.Me.Find<HtmlNavigation>());

        public IReadWriteValuePageModel<string, IRegisterPageModel> Username
            => this.Me
                   .Find<HtmlEdit>(HtmlEdit.PropertyNames.ControlDefinition, "username", PropertyExpressionOperator.Contains)
                   .AsPageModel(this);

        public IValueablePageModel<string, IRegisterPageModel> Password
            => this.Me
                   .Find<HtmlEdit>(HtmlEdit.PropertyNames.ControlDefinition, "password", PropertyExpressionOperator.Contains)
                   .AsPageModel(this);

        public IValueablePageModel<string, IRegisterPageModel> ConfirmPassword
            => this.Me
                   .Find<HtmlEdit>(HtmlEdit.PropertyNames.ControlDefinition, "confirmPassword", PropertyExpressionOperator.Contains)
                   .AsPageModel(this);

        public IClickablePageModel<ILayoutPageModel> Register
            => this.Me
                   .Find<HtmlButton>()
                   .AsPageModel(new LayoutPageModelBase(this.parent));
    }

    public class RegisterOrchestations : IRegisterOrchestrations
    {
        public IRegisterPageModel Model { get; }

        public RegisterOrchestations(IRegisterPageModel registerModel)
        {
            this.Model = registerModel;
        }

        public ILayoutPageModel Register(string username, string password)
        {
            return
            this.Model
                .Username.SetValue(username)
                .Password.SetValue(password)
                .ConfirmPassword.SetValue(password)
                .Register.Click();
        }
    }
}