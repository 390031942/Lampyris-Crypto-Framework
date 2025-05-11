#pragma once
// QT Include(s)
#include <QPixmap>
#include <QLabel>

class ControlFactory {
public:
	static QLabel* createVerticalSplitterLabel(QWidget* parent, QSize size = QSize(2,20));
};

