// Project Include(s)
#include "DateTimeUtil.h"

// QT Include(s)
#include <QTimeZone>

QDateTime DateTimeUtil::fromUtcTimestamp(qint64 timestamp) {
#if QT_VERSION >= QT_VERSION_CHECK(5, 10, 0)
    // 使用新的 QTimeZone 接口（Qt 5.10 及更高版本）
    return QDateTime::fromMSecsSinceEpoch(timestamp, QTimeZone::utc());
#else
    // 使用旧的 Qt::TimeSpec 接口（Qt 5.9 及更早版本）
    return QDateTime::fromMSecsSinceEpoch(timestamp, Qt::UTC);
#endif
}

// 将 QDateTime 转换为 UTC 毫秒时间戳
qint64 DateTimeUtil::toUtcTimestamp(const QDateTime& dateTime) {
    // 使用 QDateTime 的 toMSecsSinceEpoch 方法，获取毫秒时间戳
    return dateTime.toMSecsSinceEpoch();
}
