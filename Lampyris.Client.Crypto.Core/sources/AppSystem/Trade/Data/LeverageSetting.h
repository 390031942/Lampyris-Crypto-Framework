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

class LeverageSetting {
public:
	// ���׶�
	std::string symbol;

	// ���׶�
	int leverage;

    LeverageBean ToBean() {
        LeverageBean bean;
        bean.set_symbol(this->symbol);
        bean.set_leverage(this->leverage);
        return bean;
    }
};