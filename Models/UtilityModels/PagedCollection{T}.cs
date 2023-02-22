using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;

namespace TimesheetBE.Models.UtilityModels {
    public class PagedCollection<T> {
        public static PagedCollection<T> Create(
           Link self, T[] items, int size, PagingOptions pagingOptions)
           => Create<PagedCollection<T>>(self, items, size, pagingOptions);

        public static TResponse Create<TResponse>(
            Link self, T[] items, int size, PagingOptions pagingOptions)
            where TResponse : PagedCollection<T>, new()
            => new TResponse {
                Self = self,
                Value = items,
                Size = size,
                Offset = pagingOptions.Offset,
                Limit = pagingOptions.Limit,
                NextOffset = GetNextOffset(pagingOptions),
                PreviousOffset = GetPreviousOffset(size,pagingOptions),
                First = self,
                Next = GetNextLink(self, size, pagingOptions),
                Previous = GetPreviousLink(self, size, pagingOptions),
                Last = GetLastLink(self, size, pagingOptions)
            };

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Offset { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Limit { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? NextOffset { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PreviousOffset { get; set; }

        public int Size { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link First { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Previous { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Next { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Last { get; set; }
        public Link Self { get; set; }
        public T[] Value { get; set; }

        private static Link GetNextLink(
            Link self, int size, PagingOptions pagingOptions) {
            if (pagingOptions?.Limit == null) return null;
            if (pagingOptions?.Offset == null) return null;

            var limit = pagingOptions.Limit.Value;
            var offset = pagingOptions.Offset.Value;

            var nextPage = offset + limit;
            if (nextPage >= size) {
                return null;
            }

            var parameters = new RouteValueDictionary(self.RouteValues) {
                ["limit"] = limit,
                ["offset"] = nextPage
            };

            var newLink = Link.ToCollection(self.RouteName, parameters);
            return newLink;
        }

        private static Link GetLastLink(Link self, int size, PagingOptions pagingOptions) {
            if (pagingOptions?.Limit == null) return null;

            var limit = pagingOptions.Limit.Value;

            if (size <= limit) return null;

            var offset = Math.Ceiling((size - (double)limit) / limit) * limit;

            var parameters = new RouteValueDictionary(self.RouteValues) {
                ["limit"] = limit,
                ["offset"] = offset
            };
            var newLink = Link.ToCollection(self.RouteName, parameters);

            return newLink;
        }

        private static int? GetNextOffset(PagingOptions pagingOptions)
        {
            if (pagingOptions?.Limit == null) return null;
            if (pagingOptions?.Offset == null) return null;

            var limit = pagingOptions.Limit.Value;
            var offset = pagingOptions.Offset.Value; 

            return offset + limit;
        }

        private static int? GetPreviousOffset(int size,PagingOptions pagingOptions)
        {
            if (pagingOptions?.Limit == null) return null;
            if (pagingOptions?.Offset == null) return null;

            var limit = pagingOptions.Limit.Value;
            var offset = pagingOptions.Offset.Value;

            if (offset == 0) {
                return null;
            }

            if (offset > size) {
                return (int)Math.Ceiling((size - (double)limit) / limit) * limit;
            }

            var previousPage = Math.Max(offset - limit, 0);

            if (previousPage <= 0) {
                return null;
            }

            return previousPage;
        }

        private static Link GetPreviousLink(Link self, int size, PagingOptions pagingOptions) {
            if (pagingOptions?.Limit == null) return null;
            if (pagingOptions?.Offset == null) return null;

            var limit = pagingOptions.Limit.Value;
            var offset = pagingOptions.Offset.Value;

            if (offset == 0) {
                return null;
            }

            if (offset > size) {
                return GetLastLink(self, size, pagingOptions);
            }

            var previousPage = Math.Max(offset - limit, 0);

            if (previousPage <= 0) {
                return self;
            }

            var parameters = new RouteValueDictionary(self.RouteValues) {
                ["limit"] = limit,
                ["offset"] = previousPage
            };
            var newLink = Link.ToCollection(self.RouteName, parameters);

            return newLink;
        }
    }
}