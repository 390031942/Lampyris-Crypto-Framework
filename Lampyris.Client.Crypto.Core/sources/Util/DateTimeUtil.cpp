// Project Include(s)
#include "DateTimeUtil.h"

// �� UTC ����ʱ���ת��Ϊ QDateTime
 QDateTime DateTimeUtil::fromUtcTimestamp(qint64 timestamp) {
   // ʹ�� QDateTime �� fromMSecsSinceEpoch ������������ʱ���ת��Ϊ UTC ʱ��
    return QDateTime::fromMSecsSinceEpoch(timestamp, Qt::UTC);
}

// �� QDateTime ת��Ϊ UTC ����ʱ���
qint64 DateTimeUtil::toUtcTimestamp(const QDateTime& dateTime) {
    // ʹ�� QDateTime �� toMSecsSinceEpoch ��������ȡ����ʱ���
    return dateTime.toMSecsSinceEpoch();
}
