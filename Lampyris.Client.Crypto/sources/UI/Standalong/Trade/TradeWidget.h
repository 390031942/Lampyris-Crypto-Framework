#pragma once

// Project Include(s)
#include "UI/Common/BetterLineEdit.h"
#include "UI/Common/CustomSlider.h"
#include "Util/LayoutBuilder.h"

// QT Include(s)
#include <QLabel>
#include <QValidator>

class TradeWidget : public QWidget {
	Q_OBJECT
public:
	TradeWidget(QWidget* parent = Q_NULLPTR) {
		setupLayout();
	}
private:
	void setupLayout();
};