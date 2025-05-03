// Project Include(s)
#include "DateTimeUtil.h"

// 将 UTC 毫秒时间戳转换为 QDateTime
 QDateTime DateTimeUtil::fromUtcTimestamp(qint64 timestamp) {
   // 使用 QDateTime 的 fromMSecsSinceEpoch 方法，将毫秒时间戳转换为 UTC 时间
    return QDateTime::fromMSecsSinceEpoch(timestamp, Qt::UTC);
}

// 将 QDateTime 转换为 UTC 毫秒时间戳
qint64 DateTimeUtil::toUtcTimestamp(const QDateTime& dateTime) {
    // 使用 QDateTime 的 toMSecsSinceEpoch 方法，获取毫秒时间戳
    return dateTime.toMSecsSinceEpoch();
}
