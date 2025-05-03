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
    // 订单ID
    long long orderId;

    // 订单基本信息
    OrderInfo orderInfo;

    // 订单状态
    OrderStatus status;

    // 平均成交价
    double avgFilledPrice;

    // 成交数量
    double filledQuantity;
};