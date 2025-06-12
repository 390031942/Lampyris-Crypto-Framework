#pragma once
// QT Include(s)
#include <QWidget>

// Project Include(s)
#include "Const/UIDisplayMode.h"

class CrossPlatformWidget:public QWidget {
public:
	explicit CrossPlatformWidget(UIDisplayMode displayMode, QWidget* parent = NULL)
		:QWidget(parent) { }
protected:
	virtual void setupDisplayMode(UIDisplayMode displayMode) = 0;
};