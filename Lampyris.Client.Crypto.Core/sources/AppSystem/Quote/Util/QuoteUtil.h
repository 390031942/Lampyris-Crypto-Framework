// Project Include(s)
#include "Collections/BidirectionalDictionary.h"
#include "../Const/BarSize.h"
#include "Util/DateTimeUtil.h"

// QT Include(s)
#include <QDateTime>

class QuoteUtil {
private:
	const static BidirectionalDictionary<BarSize, std::string> ms_barSizeDictionary;
public:
	// 将 BarSize 转换为字符串
	static std::string toStdString(BarSize barSize);

	// 将字符串转换为 BarSize
	static BarSize toBarSize(const std::string& barSizeString);

	/// <summary>
	/// 获取 BarSize 对应的时间间隔（以毫秒为单位）
	/// </summary>
	static qint64 getIntervalMs(BarSize barSize);

	// 辅助函数：计算对齐后的时间戳
	static qint64 alignToInterval(const QDateTime& dateTime, BarSize barSize, bool ceil);

	// 将 QDateTime 向下对齐到最近的时间间隔起点
	static QDateTime floorToIntervalStart(const QDateTime& dateTime, BarSize barSize);

	// 将 QDateTime 向上对齐到最近的时间间隔起点
	static QDateTime ceilToIntervalStart(const QDateTime& dateTime, BarSize barSize);

	// 根据上架时间到指定时间的区间，和barSize计算其中包含的k线数目
	static int calculateCandleCount(const QDateTime& onBoardTime, const QDateTime& endDateTime, BarSize barSize);
};