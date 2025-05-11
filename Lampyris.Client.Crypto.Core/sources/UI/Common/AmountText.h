// QT Include(s)
#include <QApplication>
#include <QLabel>
#include <QString>
#include <QVBoxLayout>
#include <QWidget>

class AmountText : public QLabel {
    Q_OBJECT
public:
    explicit AmountText(QWidget* parent = nullptr);
    void     setValue(double amount, QString unit);
private:
    QString  formatNumberWithCommas(double value) const;
    void     updateText();
    double   m_amount;
    QString  m_unit;
};

