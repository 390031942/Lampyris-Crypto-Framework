#pragma once

// STD Include(s)
#include <string>
#include <vector>
#include <stdexcept>
#include <iostream>

// Project Include(s)
#include "Protocol/Protocols.h"
#include "OrderInfo.h"

class OrderStatusInfo {
public:
    // ����ID
    long long orderId;

    // ����������Ϣ
    OrderInfo orderInfo;

    // ����״̬
    OrderStatus status;

    // ƽ���ɽ���
    double avgFilledPrice;

    // �ɽ�����
    double filledQuantity;
};