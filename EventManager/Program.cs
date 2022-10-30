using System;
using System.Collections.Generic;
using System.Linq;

namespace EventManager
{
    public class Event
    {
        public string Name { get; set; }
        public string City { get; set; }
    }
    public class Customer
    {
        public string Name { get; set; }
        public string City { get; set; }
    }

    //public class CityDistanceRequest
    //{
    //    public string FromCity { get; set; }
    //    public List<string> ToCities { get; set; }
    //}
    //public class CityDistanceResponse
    //{
    //    public string ToCity { get; set; }
    //    public int Distance { get; set; }
    //}
    internal class Program
    {
        static void Main(string[] args)
        {
            var events = new List<Event>
            {
                new Event{ Name = "Phantom of the Opera", City = "New York"},
                new Event{ Name = "Metallica", City = "Los Angeles"},
                new Event{ Name = "Metallica", City = "New York"},
                new Event{ Name = "Metallica", City = "Boston"},
                new Event{ Name = "LadyGaGa", City = "New York"},
                new Event{ Name = "LadyGaGa", City = "Boston"},
                new Event{ Name = "LadyGaGa", City = "Chicago"},
                new Event{ Name = "LadyGaGa", City = "San Francisco"},
                new Event{ Name = "LadyGaGa", City = "Washington"}
            };
            //1. find out all events that are in cities of customer then add to email.
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };

            //Q1. What should be your approach to getting the list of events?
            /*
             Using LINQ to Sql to filter list of events where city events equal customer city
            */
            var customerApplicableEvent = from result in events
                                          where result.City.Equals(customer.City, StringComparison.OrdinalIgnoreCase)
                                          select result;
            // 1. TASK
            //Q2. How would you call the AddToEmail method in order to send the events in an email?
            /*
             Using iteration, foreach on each of the customer applicable event.
            */
            foreach (var @event in customerApplicableEvent)
            {
                AddToEmail(customer, @event);
            }


            var customers = new List<Customer>{
                           new Customer{ Name = "Nathan", City = "New York"},
                           new Customer{ Name = "Bob", City = "Boston"},
                           new Customer{ Name = "Cindy", City = "Chicago"},
                           new Customer{ Name = "Lisa", City = "Los Angeles"}
                           };

            //1.What should be your approach to getting the distance between the customer’s city and the other cities on the list ?
            //2.How would you get the 5 closest events and how would you send them to the client in an email?

            var customerAndCityEvents = customers.Select(c => new
            {
                customer = c,
                cityEvents = events.Select(e => new { distance = c.City == e.City ? 0 : GetDistance(c.City, e.City), customerEvent = e })
                               .OrderBy(x => x.distance)
                               .Take(5)
            }).ToList();
            foreach (var (cityCustomers, cityEvents) in from cityCustomer in customerAndCityEvents
                                                        from cityEvent in cityCustomer.cityEvents
                                                        select (cityCustomer, cityEvent))
            {
                AddToEmail(cityCustomers.customer, cityEvents.customerEvent);
            }
            //Q5
            var customerEventsWithPrice = customers.Select(c => new
            {
                customer = c,
                cityEvents = events.Select(e => new { distance = c.City == e.City ? 0 : GetDistance(c.City, e.City), customerEvent = e, price = GetPrice(e) })
                               .OrderBy(x => x.distance)
                               .ThenBy(p => p.price)
                               .Take(5)
            }).ToList();

            foreach (var (cityCustomers, cityEvents) in from cityCustomer in customerEventsWithPrice
                                                        from cityEvent in cityCustomer.cityEvents
                                                        select (cityCustomer, cityEvent))
            {
                AddToEmail(cityCustomers.customer, cityEvents.customerEvent, cityEvents.price);
            }
        }

        // You do not need to know how these methods work
        static void AddToEmail(Customer c, Event e, int? price = null)
        {
            var distance = GetDistance(c.City, e.City);
            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}"
            + (distance > 0 ? $" ({distance} miles away)" : "")
            + (price.HasValue ? $" for ${price}" : ""));
        }

        static int GetPrice(Event e)
        {
            return (AlphebiticalDistance(e.City, "") + AlphebiticalDistance(e.Name, "")) / 10;
        }

        private static int AlphebiticalDistance(string s, string t)
        {
            var result = 0;
            var i = 0;
            for (i = 0; i < Math.Min(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 1 i={i} {s.Length} {t.Length}");
                result += Math.Abs(s[i] - t[i]);
            }
            for (; i < Math.Max(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 2 i={i} {s.Length} {t.Length}");
                result += s.Length > t.Length ? s[i] : t[i];
            }
            return result;
        }
        //4. If the GetDistance method can fail, we don't want the process to fail. What can be done? Code it.
        //(Ask clarifying questions to be clear about what is expected business-wise)
        static int GetDistance(string fromCity, string toCity)
        {
            try
            {
                return AlphebiticalDistance(fromCity, toCity);
            }
            catch (Exception)
            {
                //Question: Does the business want to exclude the event when this method fail?
                return -1;
            }
        }

        //Q3. If the GetDistance method is an API call which could fail or is too expensive, how will u improve the code written in 2? Write the code.
        private static Dictionary<string, int> GetDistanceRefactoring(Dictionary<string, List<string>> cityDistanceRequest)
        {
            var cityDistances = new Dictionary<string, int>();
            foreach (var (item, toCity) in from item in cityDistanceRequest
                                           from toCity in item.Value
                                           select (item, toCity))
            {
                if (item.Key == toCity)
                {
                    cityDistances.Add(toCity, 0);
                    continue;
                }

                cityDistances.Add(toCity, AlphebiticalDistance(item.Key, toCity));
            }

            return cityDistances;
        }
    }
}
