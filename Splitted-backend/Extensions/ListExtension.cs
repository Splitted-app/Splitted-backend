namespace Splitted_backend.Extensions
{
    public static class ListExtension
    {
        public static decimal Percentile(this List<decimal> sortedDecimals, decimal p)
        {

            if (p >= 100M) return sortedDecimals[sortedDecimals.Count - 1];

            decimal leftNumber, rightNumber = 0M;
            decimal position = (sortedDecimals.Count + 1) * p / 100M;
            decimal n = p / 100M * (sortedDecimals.Count - 1) + 1M;

            if (position >= 1)
            {
                leftNumber = sortedDecimals[(int)Math.Floor(n) - 1];
                rightNumber = sortedDecimals[(int)Math.Floor(n)];
            }
            else
            {
                leftNumber = sortedDecimals[0];
                rightNumber = sortedDecimals[1];
            }

            if (leftNumber == rightNumber)
                return leftNumber;

            decimal part = n - Math.Floor(n);
            return leftNumber + part * (rightNumber - leftNumber);
        }
    }
}
