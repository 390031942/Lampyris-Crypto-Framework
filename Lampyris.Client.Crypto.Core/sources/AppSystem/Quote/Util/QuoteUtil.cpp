// Project Include(s)
#include "QuoteUtil.h"

// 静态成员初始化
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

// 将 BarSize 转换为字符串
std::string QuoteUtil::toStdString(BarSize barSize) {
	return ms_barSizeDictionary.getByKey(barSize);
}

// 将字符串转换为 BarSize
BarSize QuoteUtil::toBarSize(const std::string& barSizeString) {
	return ms_barSizeDictionary.getByValue(barSizeString);
}

/// <summary>
/// 获取 BarSize 对应的时间间隔（以毫秒为单位）
/// </summary>
qint64 QuoteUtil::getIntervalMs(BarSize barSize) {
	switch (barSize) {
	case _1m:  return 1 * 60 * 1000;          // 1 分钟
	case _3m:  return 3 * 60 * 1000;          // 3 分钟
	case _5m:  return 5 * 60 * 1000;          // 5 分钟
	case _15m: return 15 * 60 * 1000;         // 15 分钟
	case _30m: return 30 * 60 * 1000;         // 30 分钟
	case _1H:  return 1 * 60 * 60 * 1000;     // 1 小时
	case _2H:  return 2 * 60 * 60 * 1000;     // 2 小时
	case _4H:  return 4 * 60 * 60 * 1000;     // 4 小时
	case _6H:  return 6 * 60 * 60 * 1000;     // 6 小时
	case _12H: return 12 * 60 * 60 * 1000;    // 12 小时
	case _1D:  return 1 * 24 * 60 * 60 * 1000; // 1 天
	case _3D:  return 3 * 24 * 60 * 60 * 1000; // 3 天
	case _1W:  return 7 * 24 * 60 * 60 * 1000; // 1 周
	}
	return 0L;
}

// 辅助函数：计算对齐后的时间戳
qint64 QuoteUtil::alignToInterval(const QDateTime& dateTime, BarSize barSize, bool ceil) {
	int intervalInSeconds = QuoteUtil::getIntervalMs(barSize);
	if (intervalInSeconds == 0) {
		std::cerr << "Error: Invalid BarSize value." << std::endl;
		return dateTime.toSecsSinceEpoch();
	}

	qint64 timestamp = dateTime.toSecsSinceEpoch();
	if (ceil) {
		// 向上对齐
		return timestamp + (intervalInSeconds - (timestamp % intervalInSeconds)) % intervalInSeconds;
	}
	else {
		// 向下对齐
		return timestamp - (timestamp % intervalInSeconds);
	}
}

// 将 QDateTime 向下对齐到最近的时间间隔起点
QDateTime QuoteUtil::floorToIntervalStart(const QDateTime& dateTime, BarSize barSize) {
	qint64 alignedTimestamp = alignToInterval(dateTime, barSize, false);
	return DateTimeUtil::fromUtcTimestamp(alignedTimestamp);
}

// 将 QDateTime 向上对齐到最近的时间间隔起点
QDateTime QuoteUtil::ceilToIntervalStart(const QDateTime& dateTime, BarSize barSize) {
	qint64 alignedTimestamp = alignToInterval(dateTime, barSize, true);
	return DateTimeUtil::fromUtcTimestamp(alignedTimestamp);
}

// 根据上架时间到指定时间的区间，和barSize计算其中包含的k线数目
int QuoteUtil::calculateCandleCount(const QDateTime& onBoardTime, const QDateTime& endDateTime, BarSize barSize) {
	// 将 QDateTime 转换为时间戳（秒）
	QDateTime alignedOnBoardTimestamp = floorToIntervalStart(onBoardTime, barSize);
	qint64 onBoardTimestamp = DateTimeUtil::toUtcTimestamp(alignedOnBoardTimestamp);
	qint64 endTimestamp = DateTimeUtil::toUtcTimestamp(endDateTime);

	// 检查时间范围是否有效
	if (endTimestamp < onBoardTimestamp) {
		//  "Error: End time must be greater than on-board time."
		return 0;
	}

	// 获取时间间隔（秒）
	int barSizeInSeconds = QuoteUtil::getIntervalMs(barSize) / 1000;
	if (barSizeInSeconds == 0) {
		// "Error: Invalid BarSize value."
		return 0;
	}

	// 计算时间差（秒）
	qint64 timeDifference = endTimestamp - onBoardTimestamp;

	// 计算 K 线数量
	int kLineCount = static_cast<int>(std::ceil(static_cast<double>(timeDifference) / barSizeInSeconds)) + 1;

	return kLineCount;
}
