#include "QuoteListItem.h"

// QT Include(s)
#include <QApplication>

QuoteListItem::QuoteListItem(DisplayMode mode, QWidget* parent) : QWidget(parent) {
    // ��ʼ���ؼ�
    m_symbol = new QLabel(this);
    m_24hVolumeCurrency = new AmountText(this);
    m_price = new PriceText(this);
    m_percentage = new PercentageDisplayText(this);
    m_riseSpeed = new PercentageDisplayText(this);

    // ����ģʽ����UI����
    setupUI(mode);

    // ��������
    QFont font = QApplication::font();

    // ���׶�����
    font.setPixelSize(18);
    m_symbol->setFont(font);
    m_price->setFont(font);
    m_percentage->setFont(font);

    m_24hVolumeCurrency->setStyleSheet("color: #8A8E97;");
    m_percentage->setDisplayMode(PercentageDisplayText::BackgroundColorMode);
    m_riseSpeed->setDisplayMode(PercentageDisplayText::FontColorMode);
    m_percentage->setFixedWidth(100);

    // �·���24Сʱ�ɽ�������
    font.setPixelSize(12);
    m_24hVolumeCurrency->setFont(font);
}

void QuoteListItem::refresh(const QuoteTickerDataPtr tickerData, double minTick) {
    if (tickerData != nullptr) {
        // ��������
        m_symbol->setText(tickerData->symbol);
        m_24hVolumeCurrency->setValue(tickerData->currency);
        m_price->setValue(tickerData->price);
        m_price->setMinTick(minTick);
        m_percentage->setValue(tickerData->changePerc);
        m_riseSpeed->setValue(tickerData->riseSpeed);
    }
}

void QuoteListItem::setupUI(DisplayMode mode) {
    // ��ղ���
    QLayout* existingLayout = this->layout();
    if (existingLayout) {
        delete existingLayout;
    }

    // ��������
    QHBoxLayout* hLayout = new QHBoxLayout(this);
    hLayout->setContentsMargins(5, 0, 0, 0);

    if (mode == SearchListMode) {
        // �����б�ģʽ����: | ���Ŷ� | �۸� | �ǵ��� |
        hLayout->addWidget(m_symbol);
        hLayout->addWidget(m_price);
        hLayout->addWidget(m_percentage);
    }
    else if (mode == MobileListMode) {
        // �ƶ��������б�ģʽ����: 
        // ���Ŷ�     |  �۸�  |    �ǵ���
        // 24h�ɽ���  |  ����  |
        QVBoxLayout* vLayout = new QVBoxLayout;
        vLayout->setContentsMargins(0, 0, 0, 0);
        QVBoxLayout* vLayout2 = new QVBoxLayout(this);
        vLayout2->setContentsMargins(0, 0, 0, 0);

        vLayout->setSpacing(0);
        vLayout2->setSpacing(0);
        vLayout->addWidget(m_symbol);
        vLayout->addWidget(m_24hVolumeCurrency);
        hLayout->addLayout(vLayout);
        hLayout->addItem(new QSpacerItem(40, 20, QSizePolicy::Expanding, QSizePolicy::Minimum));
        vLayout2->addWidget(m_price);
        vLayout2->addWidget(m_riseSpeed);
        hLayout->addLayout(vLayout2);
        hLayout->addItem(new QSpacerItem(40, 20, QSizePolicy::Expanding, QSizePolicy::Minimum));
        hLayout->addWidget(m_percentage);
    }
    else if (mode == PCListMode) {
        // PC�������б�ģʽ����: | ���Ŷ� | �۸� | �ɽ��� | �ǵ��� | ���� |
        hLayout->addWidget(m_symbol);
        hLayout->addWidget(m_price);
        hLayout->addWidget(m_24hVolumeCurrency);
        hLayout->addWidget(m_percentage);
        hLayout->addWidget(m_riseSpeed);
    }
}
