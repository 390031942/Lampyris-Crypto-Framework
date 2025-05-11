#pragma once
// QT Include(s)
#include <QPixmap>
#include <QLabel>
#include <QSpacerItem>

class ControlFactory {
public:
	static QLabel*      createVerticalSplitterLabel(QWidget* parent, QSize size = QSize(2,20));
	static QSpacerItem* createHorizontalSpacerItem();
};

