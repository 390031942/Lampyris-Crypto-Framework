#pragma once

// QT Include(s)
#include <QApplication>
#include <QLabel>
#include <QString>
#include <QVBoxLayout>
#include <QWidget>

// STD Include(s)
#include <cmath>

class PriceText : public QLabel {
    Q_OBJECT
public:
    explicit PriceText(QWidget* parent = nullptr);
    void     setValue(double value);
    void     setMinTick(double minTick);
private:
    double   m_value;  
    double   m_minTick;
    QString  formatNumberWithCommas(double value, int decimalPlaces) const;
    void     updateText();
};
