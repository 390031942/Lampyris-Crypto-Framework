#include <QApplication>
#include <QNetworkAccessManager>
#include <QNetworkRequest>
#include <QNetworkReply>
#include <QUrl>
#include <QUrlQuery>
#include <QDateTime>
#include <QThreadPool>
#include <QRunnable>
#include <QJsonDocument>
#include <QJsonArray>
#include <QDebug>
#include <QMutex>
#include <QWaitCondition>
#include <vector>
#include <memory>
#include <QSslSocket>

#include "QuoteCandleData.h"

class BinanceAPI : public QObject {
    Q_OBJECT

public:
    explicit BinanceAPI(QObject* parent = nullptr) : QObject(parent) {
        manager = new QNetworkAccessManager(this);
    }

    // fetchKlines ֧�ֶ��̷ֶ߳�����
    void fetchKlines(const QString& symbol, const QString& interval, int limit, const QDateTime& startTime = QDateTime(), const QDateTime& endTime = QDateTime()) {
        executeKlinesRequest(symbol, interval, limit, startTime, endTime);
    }

    // fetchKlinesFromEndTime ֧�ֶ��̷ֶ߳�����
    void fetchKlinesFromEndTime(const QString& symbol, const QString& interval, int limit, const QDateTime& endTime) {
        // ����ÿ�� interval �ĳ���ʱ�䣨�Ժ���Ϊ��λ��
        qint64 intervalDuration = getIntervalDuration(interval);
        if (intervalDuration == 0) {
            qDebug() << "Invalid interval:" << interval;
            return;
        }

        // ���㿪ʼʱ��
        qint64 startTimeMs = endTime.toMSecsSinceEpoch() - (limit * intervalDuration);
        QDateTime startTime = QDateTime::fromMSecsSinceEpoch(startTimeMs);

        // ����ͨ�õ�ִ�к���
        executeKlinesRequest(symbol, interval, limit, startTime, endTime);
    }

signals:
    // ����������ɺ����ź�
    void dataFetched(const std::vector<QuoteCandleDataPtr>& dataList);

private:
    QNetworkAccessManager* manager;

    // ͨ�õ� K ������ִ�к���
    void executeKlinesRequest(const QString& symbol, const QString& interval, int limit, const QDateTime& startTime, const QDateTime& endTime) {
        if (!startTime.isValid() || !endTime.isValid()) {
            // ���δ�ṩʱ�䷶Χ��ֱ���������µ� limit ������
            sendKlinesRequest(symbol, interval, limit, QDateTime(), QDateTime());
        }
        else if (limit <= 1000) {
            // ��� limit С�ڵ��� 1000��ֱ�ӷ��͵�������
            sendKlinesRequest(symbol, interval, limit, startTime, endTime);
        }
        else {
            // ��� limit ���� 1000�����зֶ�����
            int numSegments = (limit + 999) / 1000; // ����ֶ�����
            qint64 intervalDuration = getIntervalDuration(interval);
            if (intervalDuration == 0) {
                qDebug() << "Invalid interval:" << interval;
                return;
            }

            QList<QDateTime> segmentStartTimes;
            QList<QDateTime> segmentEndTimes;

            // ����ÿ�ε�ʱ�䷶Χ
            for (int i = 0; i < numSegments; ++i) {
                QDateTime segmentEndTime = endTime.addMSecs(-i * 1000 * intervalDuration);
                QDateTime segmentStartTime = segmentEndTime.addMSecs(-1000 * intervalDuration);
                segmentStartTimes.append(segmentStartTime);
                segmentEndTimes.append(segmentEndTime);
            }

            // ���̴߳���
            QThreadPool threadPool;
            std::vector<QuoteCandleDataPtr> results;
            QMutex mutex;
            QWaitCondition waitCondition;

            for (int i = 0; i < numSegments; ++i) {
                auto runnable = new KlineFetcherRunnable(symbol, interval, 1000, segmentStartTimes[i], segmentEndTimes[i], manager, &results, &mutex, &waitCondition);
                threadPool.start(runnable);
            }

            // �ȴ������߳����
            threadPool.waitForDone();

            // �����źţ�֪ͨ���������
            emit dataFetched(results);
        }
    }

    // ��������ķ��ͺ���
    void sendKlinesRequest(const QString& symbol, const QString& interval, int limit, const QDateTime& startTime, const QDateTime& endTime) {
        qDebug() << "Supports SSL:" << QSslSocket::supportsSsl();
        qDebug() << "SSL Library Version:" << QSslSocket::sslLibraryVersionString();
        qDebug() << "SSL Build Version:" << QSslSocket::sslLibraryBuildVersionString();
        
        // �������� URL
        QUrl url("https://fapi.binance.com/fapi/v1/klines");
        QUrlQuery query;
        query.addQueryItem("symbol", symbol);
        query.addQueryItem("interval", interval);
        query.addQueryItem("limit", QString::number(limit));
        if (startTime.isValid()) {
            query.addQueryItem("startTime", QString::number(startTime.toMSecsSinceEpoch()));
        }
        if (endTime.isValid()) {
            query.addQueryItem("endTime", QString::number(endTime.toMSecsSinceEpoch()));
        }
        url.setQuery(query);

        QNetworkRequest request(url);

        // ���� GET ����
        QNetworkReply* reply = manager->get(request);
        connect(reply, &QNetworkReply::finished, this, [this, reply]() {
            if (reply->error() != QNetworkReply::NoError) {
                qDebug() << "Error:" << reply->errorString();
                reply->deleteLater();
                return;
            }

            // ���� JSON ����
            QByteArray responseData = reply->readAll();
            QJsonDocument jsonDoc = QJsonDocument::fromJson(responseData);
            if (!jsonDoc.isArray()) {
                qDebug() << "Invalid JSON format";
                reply->deleteLater();
                return;
            }

            QJsonArray jsonArray = jsonDoc.array();
            std::vector<QuoteCandleDataPtr> dataList = parseKlinesData(jsonArray);

            // �����źţ�֪ͨ���������
            emit dataFetched(dataList);

            reply->deleteLater();
        });
    }

    // ���� JSON ����Ϊ QuoteCandleDataPtr �б�
    std::vector<QuoteCandleDataPtr> parseKlinesData(const QJsonArray& jsonArray) {
        std::vector<QuoteCandleDataPtr> dataList;
        for (const QJsonValue& value : jsonArray) {
            QJsonArray item = value.toArray();
            if (item.size() < 6) continue;

            auto data = std::make_shared<QuoteCandleData>();
            data->dateTime = QDateTime::fromMSecsSinceEpoch(item[0].toVariant().toLongLong());
            data->open = item[1].toString().toDouble();
            data->high = item[2].toString().toDouble();
            data->low = item[3].toString().toDouble();
            data->close = item[4].toString().toDouble();
            data->volume = item[5].toString().toDouble();
            if (item.size() > 7) {
                data->currency = item[7].toString().toDouble();
            }

            dataList.push_back(data);
        }
        return dataList;
    }

    // ��ȡ interval �ĳ���ʱ�䣨�Ժ���Ϊ��λ��
    qint64 getIntervalDuration(const QString& interval) {
        static const QMap<QString, qint64> intervalMap = {
            {"1m", 60 * 1000},
            {"3m", 3 * 60 * 1000},
            {"5m", 5 * 60 * 1000},
            {"15m", 15 * 60 * 1000},
            {"30m", 30 * 60 * 1000},
            {"1h", 60 * 60 * 1000},
            {"2h", 2 * 60 * 60 * 1000},
            {"4h", 4 * 60 * 60 * 1000},
            {"6h", 6 * 60 * 60 * 1000},
            {"8h", 8 * 60 * 60 * 1000},
            {"12h", 12 * 60 * 60 * 1000},
            {"1d", 24 * 60 * 60 * 1000},
            {"3d", 3 * 24 * 60 * 60 * 1000},
            {"1w", 7 * 24 * 60 * 60 * 1000},
            {"1M", 30 * 24 * 60 * 60 * 1000}
        };

        return intervalMap.value(interval, 0);
    }

    // ���߳�������
    class KlineFetcherRunnable : public QRunnable {
    public:
        KlineFetcherRunnable(const QString& symbol, const QString& interval, int limit, const QDateTime& startTime, const QDateTime& endTime, QNetworkAccessManager* manager, std::vector<QuoteCandleDataPtr>* results, QMutex* mutex, QWaitCondition* waitCondition)
            : symbol(symbol), interval(interval), limit(limit), startTime(startTime), endTime(endTime), manager(manager), results(results), mutex(mutex), waitCondition(waitCondition) {}

        void run() override {
            // �������� URL
            QUrl url("https://fapi.binance.com/fapi/v1/klines");
            QUrlQuery query;
            query.addQueryItem("symbol", symbol);
            query.addQueryItem("interval", interval);
            query.addQueryItem("limit", QString::number(limit));
            query.addQueryItem("startTime", QString::number(startTime.toMSecsSinceEpoch()));
            query.addQueryItem("endTime", QString::number(endTime.toMSecsSinceEpoch()));
            url.setQuery(query);

            QNetworkRequest request(url);

            // ���� GET ����
            QNetworkReply* reply = manager->get(request);
            QEventLoop loop;
            connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
            loop.exec();

            if (reply->error() != QNetworkReply::NoError) {
                qDebug() << "Error:" << reply->errorString();
                reply->deleteLater();
                return;
            }

            // ���� JSON ����
            QByteArray responseData = reply->readAll();
            QJsonDocument jsonDoc = QJsonDocument::fromJson(responseData);
            if (!jsonDoc.isArray()) {
                qDebug() << "Invalid JSON format";
                reply->deleteLater();
                return;
            }

            QJsonArray jsonArray = jsonDoc.array();
            auto dataList = parseKlinesData(jsonArray);

            // �������洢���
            QMutexLocker locker(mutex);
            results->insert(results->end(), dataList.begin(), dataList.end());
            waitCondition->wakeAll();

            reply->deleteLater();
        }

    private:
        QString symbol;
        QString interval;
        int limit;
        QDateTime startTime;
        QDateTime endTime;
        QNetworkAccessManager* manager;
        std::vector<QuoteCandleDataPtr>* results;
        QMutex* mutex;
        QWaitCondition* waitCondition;

        // ���� JSON ����Ϊ QuoteCandleDataPtr �б�
        std::vector<QuoteCandleDataPtr> parseKlinesData(const QJsonArray& jsonArray) {
            std::vector<QuoteCandleDataPtr> dataList;
            for (const QJsonValue& value : jsonArray) {
                QJsonArray item = value.toArray();
                if (item.size() < 6) continue;

                auto data = std::make_shared<QuoteCandleData>();
                data->dateTime = QDateTime::fromMSecsSinceEpoch(item[0].toVariant().toLongLong());
                data->open = item[1].toString().toDouble();
                data->high = item[2].toString().toDouble();
                data->low = item[3].toString().toDouble();
                data->close = item[4].toString().toDouble();
                data->volume = item[5].toString().toDouble();
                if (item.size() > 7) {
                    data->currency = item[7].toString().toDouble();
                }

                dataList.push_back(data);
            }
            return dataList;
        }
    };
};
