#pragma once

// Project Include(s)
#include <QDateTime>

class DateTimeUtil {
public:
    // �� UTC ����ʱ���ת��Ϊ QDateTime
    static QDateTime fromUtcTimestamp(qint64 timestamp);

    // �� QDateTime ת��Ϊ UTC ����ʱ���
    static qint64 toUtcTimestamp(const QDateTime& dateTime);
};
