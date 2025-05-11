#pragma once

// Project Include(s)
#include <QDateTime>

class DateTimeUtil {
public:
    // 将 UTC 毫秒时间戳转换为 QDateTime
    static QDateTime fromUtcTimestamp(qint64 timestamp);

    // 将 QDateTime 转换为 UTC 毫秒时间戳
    static qint64 toUtcTimestamp(const QDateTime& dateTime);
};
