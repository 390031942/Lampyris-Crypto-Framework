#pragma once

// QT Include(s)
#include <QApplication>
#include <QString>
#include <QVBoxLayout>
#include <QWidget>

// STD Include(s)
#include <cmath>

// Project Include(s)
#include "ColoredLabel.h"

class PriceText : public ColoredLabel {
    Q_OBJECT
public:
    explicit PriceText(QWidget* parent = nullptr);
    void     setValue(double value, double percentage = 0.0);
    void     setMinTick(double minTick);
private:
    double   m_value;
    double   m_percentage;
    double   m_minTick;
    QString  formatNumberWithCommas(double value, int decimalPlaces) const;
    void     updateText();
};
