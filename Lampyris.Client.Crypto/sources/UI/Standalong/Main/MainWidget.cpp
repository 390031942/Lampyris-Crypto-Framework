// Project Include(s)
#include "MainWidget.h"

// QT Include(s)
#include <QLayout>

MainWidget::MainWidget(QWidget *parent)
	:QWidget(parent)
	,m_titleBar(new MainTitleBar(this))
	,m_toolBar(new MainToolBar(this))
	,m_statusBar(new MainStatusBar(this))
	,m_centralWidget(new QWidget(this)) {

	setMinimumSize(QSize(1025, 600));
	// 隐藏系统标题栏
	setWindowFlags(Qt::FramelessWindowHint);

	// 布局
	QVBoxLayout* vLayout = new QVBoxLayout(this);
	this->setLayout(vLayout);

	vLayout->setContentsMargins(0, 0, 0, 0);
	vLayout->setSpacing(0);
	vLayout->addWidget(m_titleBar);

	QVBoxLayout* vLayoutRight = new QVBoxLayout(this);
	vLayoutRight->setSpacing(0);
	vLayoutRight->setContentsMargins(0, 0, 0, 0);

	QHBoxLayout* hLayout = new QHBoxLayout(this);
	hLayout->setSpacing(0);

	QWidget* widget = new QWidget(this);
	widget->setStyleSheet("QWidget { background-color: #181A20; color: white; }");
	widget->setAttribute(Qt::WA_StyledBackground); // 启用样式表背景支持

	widget->setLayout(hLayout);
	vLayout->addWidget(widget);

	hLayout->setContentsMargins(0, 0, 0, 0);
	hLayout->addWidget(m_toolBar);
	hLayout->addLayout(vLayoutRight);
	m_centralWidget->setSizePolicy(QSizePolicy::Preferred, QSizePolicy::Preferred);

	vLayoutRight->addWidget(m_centralWidget);
	vLayoutRight->addWidget(m_statusBar);

	m_toolBar->addButton(":/res/icons/home_normal.png",  ":/res/icons/home_selected.png",  "主页");
	m_toolBar->addButton(":/res/icons/quote_normal.png", ":/res/icons/quote_selected.png", "行情");
	m_toolBar->addButton(":/res/icons/trade_normal.png", ":/res/icons/trade_selected.png", "交易");
	m_toolBar->addButton(":/res/icons/asset_normal.png", ":/res/icons/asset_selected.png", "资产");
}