#pragma once

#include <QtWidgets/QMainWindow>
#include "ui_LampyrisClientCrypto.h"

class LampyrisClientCrypto : public QMainWindow
{
    Q_OBJECT

public:
    LampyrisClientCrypto(QWidget *parent = nullptr);
    ~LampyrisClientCrypto();

private:
    Ui::LampyrisClientCryptoClass ui;
};
