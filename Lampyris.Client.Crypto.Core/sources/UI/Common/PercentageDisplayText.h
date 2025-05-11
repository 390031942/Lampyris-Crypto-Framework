#pragma once

// Project Include(s)
#include <QApplication>
#include <QLabel>
#include <QVBoxLayout>
#include <QWidget>
#include <QPainter>
#include <QStyleOption>

/*
 * �Ի�������ʾ�ǵ����Ŀؼ�����ʾ��������:xxx.xx%(����2λ��Ч����),
 * ����֧�ָ����ǵ�ƽ�������������/������ɫ ��ʾΪ��Ӧ����ɫ��
 */
class PercentageDisplayText : public QLabel {
    Q_OBJECT

public:
    enum DisplayMode {
        FontColorMode,  // �����ɫģʽ
        BackgroundColorMode // ������ɫģʽ
    };

    explicit    PercentageDisplayText(QWidget* parent = nullptr);
    void        setDisplayMode(DisplayMode displayMode);
    void        setValue(double newValue);
protected:
    void        paintEvent(QPaintEvent* event) override;
private:
    QColor      backgroundColor() const;
    QString     formattedText(int decimalPlaces = 2) const;
    void        updateAppearance();

    DisplayMode m_mode;
    double      m_value;

};
