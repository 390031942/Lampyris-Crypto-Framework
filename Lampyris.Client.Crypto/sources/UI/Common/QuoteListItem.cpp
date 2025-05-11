// Project Include(s)
#include "QuoteListItem.h"

// QT Include(s)
#include <QApplication>

QuoteListItem::QuoteListItem(QWidget* parent) :QWidget(parent) {
	m_symbol = new QLabel(this);
	m_24hVolumeCurrency = new AmountText(this);
	m_price = new PriceText(this);
	m_perctange = new PercentageDisplayText(this);

	QHBoxLayout* hLayout = new QHBoxLayout(this);
	hLayout->setContentsMargins(5, 0, 0, 0);
	QVBoxLayout* vLayout = new QVBoxLayout;
	vLayout->setContentsMargins(0, 0, 0, 0);

	vLayout->setSpacing(0);
	vLayout->addWidget(m_symbol);
	vLayout->addWidget(m_24hVolumeCurrency);
	hLayout->addLayout(vLayout);
	hLayout->addItem(new QSpacerItem(40,20,QSizePolicy::Expanding,QSizePolicy::Minimum));
	hLayout->addWidget(m_price);
	hLayout->addWidget(m_perctange);

	// 字体
	QFont font = QApplication::font();

	// 交易对 字体
	font.setPixelSize(18);
	m_symbol->setFont(font);
	m_price->setFont(font);
	m_perctange->setFont(font);

	// 下方的24小时成交额
	font.setPixelSize(12);
	m_24hVolumeCurrency->setFont(font);

	// 测试数据
	m_symbol->setText("AUCTIONUSDT");
	m_24hVolumeCurrency->setValue(123789,"USDT");
	m_24hVolumeCurrency->setStyleSheet("color: #8A8E97;");
	m_price->setValue(85000.1);
	m_price->setMinTick(0.1);
	m_perctange->setValue(12.34);
	m_perctange->setDisplayMode(PercentageDisplayText::BackgroundColorMode);
}
