#pragma once

// STD Include(s)
#include <string>
#include <vector>
#include <stdexcept>
#include <iostream>

// Project Include(s)
#include "Protocol/Protocols.h"
#include "OrderInfo.h"
#include "Util/DateTimeUtil.h"

// PositionInfo ��
class LeverageBracketInfo {
public:
    // ���׶ԣ����� BTCUSDT
    std::string symbol;

    // ���׶�
    int leverage;

    // �ֲ�����
    double quantity;

    // ��ǰ�ֲ��µ������ֵ����
    double notionalCap;

    // ��ǰ�ֲ��µ������ֵ����
    double notionalFloor;
};