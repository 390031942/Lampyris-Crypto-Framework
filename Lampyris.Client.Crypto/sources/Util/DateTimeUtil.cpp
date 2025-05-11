// Project Include(s)
#include "DateTimeUtil.h"

// QT Include(s)
#include <QTimeZone>

QDateTime DateTimeUtil::fromUtcTimestamp(qint64 timestamp) {
#if QT_VERSION >= QT_VERSION_CHECK(5, 10, 0)
    // ʹ���µ� QTimeZone �ӿڣ�Qt 5.10 �����߰汾��
    return QDateTime::fromMSecsSinceEpoch(timestamp, QTimeZone::utc());
#else
    // ʹ�þɵ� Qt::TimeSpec �ӿڣ�Qt 5.9 ������汾��
    return QDateTime::fromMSecsSinceEpoch(timestamp, Qt::UTC);
#endif
}

// �� QDateTime ת��Ϊ UTC ����ʱ���
qint64 DateTimeUtil::toUtcTimestamp(const QDateTime& dateTime) {
    // ʹ�� QDateTime �� toMSecsSinceEpoch ��������ȡ����ʱ���
    return dateTime.toMSecsSinceEpoch();
}
