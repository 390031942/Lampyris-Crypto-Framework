namespace Lampyris.CSharp.Common;

public static class ListExtension
{
    public static int LowerBound<T>(this List<T> sortedList, T value) where T : IComparable<T>
    {
        int index = SortedListBinarySearch(sortedList, value, true);
        return index >= 0 ? index : ~index;
    }

    public static int UpperBound<T>(this List<T> sortedList, T value) where T : IComparable<T>
    {
        int index = SortedListBinarySearch(sortedList, value, false);
        return index >= 0 ? index : ~index;
    }

    private static int SortedListBinarySearch<T>(List<T> sortedList, T value, bool lowerBound) where T : IComparable<T>
    {
        int lower = 0;
        int upper = sortedList.Count - 1;
        int index = -1;

        while (lower <= upper)
        {
            index = lower + (upper - lower) / 2;
            var comparisonResult = sortedList[index].CompareTo(value);

            if (comparisonResult == 0)
            {
                if (lowerBound)
                {
                    // ����ҵ���ȵ�Ԫ�أ�����������Ҫ lower_bound����ֱ�ӷ���
                    return index;
                }
                else
                {
                    // ����ҵ���ȵ�Ԫ�أ�����������Ҫ upper_bound����������ұ߲�������
                    upper = index - 1;
                }
            }
            else if (comparisonResult < 0)
            {
                lower = index + 1;
            }
            else
            {
                upper = index - 1;
            }
        }

        return lowerBound ? lower : upper;
    }
}