﻿namespace More.Integration
{
    using More.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Web.OData;

    public class PeopleRepository : IReadOnlyRepository<Person>
    {
        private readonly Lazy<IQueryable<Person>> database = new Lazy<IQueryable<Person>>( CreateDatabase );

        private static IQueryable<Person> CreateDatabase()
        {
            return new[]
            {
                new Person()
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Links =
                    {
                        new Link( "receipt", new Uri( "http://localhost:26939/api/receipts(67b4e997-e004-4521-b87d-b8b4693a8043)" ) )
                    },
                    //SeoTerms =
                    //{
                    //    "Doe"
                    //}
                },
                new Person()
                {
                    Id = 2,
                    FirstName = "Bill",
                    LastName = "Mei",
                    Links =
                    {
                        new Link( "address", new Uri( "http://remote/api/addresses/3" ) ),
                        new Link( "temp", new Uri( "http://tempuri.org" ) )
                    },
                    //SeoTerms =
                    //{
                    //    "Bill"
                    //}
                }
            }.AsQueryable();
        }

        private IQueryable<Person> Database
        {
            get
            {
                return database.Value;
            }
        }

        public Task<IEnumerable<Person>> GetAsync( Func<IQueryable<Person>, IQueryable<Person>> queryShaper, CancellationToken cancellationToken ) => Task.FromResult( queryShaper( Database ).AsEnumerable() );

        public Task<TResult> GetAsync<TResult>( Func<IQueryable<Person>, TResult> queryShaper, CancellationToken cancellationToken ) => Task.FromResult( queryShaper( Database ) );
    }
}
