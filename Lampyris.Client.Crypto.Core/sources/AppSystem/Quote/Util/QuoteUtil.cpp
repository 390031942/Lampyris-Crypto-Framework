// Project Include(s)
#include "QuoteUtil.h"

// ��̬��Ա��ʼ��
const BidirectionalDictionary<BarSize, std::string> QuoteUtil::ms_barSizeDictionary = {
	{_1m, "1m"},
	{_3m, "3m"},
	{_5m, "5m"},
	{_15m, "15m"},
	{_30m, "30m"},
	{_1H, "1H"},
	{_2H, "2H"},
	{_4H, "4H"},
	{_6H, "6H"},
	{_12H, "12H"},
	{_1D, "1D"},
	{_3D, "3D"},
	{_1W, "1W"},
	{_1M, "1M"}
};

// �� BarSize ת��Ϊ�ַ���
std::string QuoteUtil::toStdString(BarSize barSize) {
	return ms_barSizeDictionary.getByKey(barSize);
}

// ���ַ���ת��Ϊ BarSize
BarSize QuoteUtil::toBarSize(const std::string& barSizeString) {
	return ms_barSizeDictionary.getByValue(barSizeString);
}

/// <summary>
/// ��ȡ BarSize ��Ӧ��ʱ�������Ժ���Ϊ��λ��
/// </summary>
qint64 QuoteUtil::getIntervalMs(BarSize barSize) {
	switch (barSize) {
	case _1m:  return 1 * 60 * 1000;          // 1 ����
	case _3m:  return 3 * 60 * 1000;          // 3 ����
	case _5m:  return 5 * 60 * 1000;          // 5 ����
	case _15m: return 15 * 60 * 1000;         // 15 ����
	case _30m: return 30 * 60 * 1000;         // 30 ����
	case _1H:  return 1 * 60 * 60 * 1000;     // 1 Сʱ
	case _2H:  return 2 * 60 * 60 * 1000;     // 2 Сʱ
	case _4H:  return 4 * 60 * 60 * 1000;     // 4 Сʱ
	case _6H:  return 6 * 60 * 60 * 1000;     // 6 Сʱ
	case _12H: return 12 * 60 * 60 * 1000;    // 12 Сʱ
	case _1D:  return 1 * 24 * 60 * 60 * 1000; // 1 ��
	case _3D:  return 3 * 24 * 60 * 60 * 1000; // 3 ��
	case _1W:  return 7 * 24 * 60 * 60 * 1000; // 1 ��
	}
	return 0L;
}

// �������������������ʱ���
qint64 QuoteUtil::alignToInterval(const QDateTime& dateTime, BarSize barSize, bool ceil) {
	int intervalInSeconds = QuoteUtil::getIntervalMs(barSize);
	if (intervalInSeconds == 0) {
		std::cerr << "Error: Invalid BarSize value." << std::endl;
		return dateTime.toSecsSinceEpoch();
	}

	qint64 timestamp = dateTime.toSecsSinceEpoch();
	if (ceil) {
		// ���϶���
		return timestamp + (intervalInSeconds - (timestamp % intervalInSeconds)) % intervalInSeconds;
	}
	else {
		// ���¶���
		return timestamp - (timestamp % intervalInSeconds);
	}
}

// �� QDateTime ���¶��뵽�����ʱ�������
QDateTime QuoteUtil::floorToIntervalStart(const QDateTime& dateTime, BarSize barSize) {
	qint64 alignedTimestamp = alignToInterval(dateTime, barSize, false);
	return DateTimeUtil::fromUtcTimestamp(alignedTimestamp);
}

// �� QDateTime ���϶��뵽�����ʱ�������
QDateTime QuoteUtil::ceilToIntervalStart(const QDateTime& dateTime, BarSize barSize) {
	qint64 alignedTimestamp = alignToInterval(dateTime, barSize, true);
	return DateTimeUtil::fromUtcTimestamp(alignedTimestamp);
}

// �����ϼ�ʱ�䵽ָ��ʱ������䣬��barSize�������а�����k����Ŀ
int QuoteUtil::calculateCandleCount(const QDateTime& onBoardTime, const QDateTime& endDateTime, BarSize barSize) {
	// �� QDateTime ת��Ϊʱ������룩
	QDateTime alignedOnBoardTimestamp = floorToIntervalStart(onBoardTime, barSize);
	qint64 onBoardTimestamp = DateTimeUtil::toUtcTimestamp(alignedOnBoardTimestamp);
	qint64 endTimestamp = DateTimeUtil::toUtcTimestamp(endDateTime);

	// ���ʱ�䷶Χ�Ƿ���Ч
	if (endTimestamp < onBoardTimestamp) {
		//  "Error: End time must be greater than on-board time."
		return 0;
	}

	// ��ȡʱ�������룩
	int barSizeInSeconds = QuoteUtil::getIntervalMs(barSize) / 1000;
	if (barSizeInSeconds == 0) {
		// "Error: Invalid BarSize value."
		return 0;
	}

	// ����ʱ���룩
	qint64 timeDifference = endTimestamp - onBoardTimestamp;

	// ���� K ������
	int kLineCount = static_cast<int>(std::ceil(static_cast<double>(timeDifference) / barSizeInSeconds)) + 1;

	return kLineCount;
}
