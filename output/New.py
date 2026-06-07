import pandas as pd
import matplotlib.pyplot as plt

df = pd.read_csv("Assets/Document/02_Data/Train/training_20260604_221518.csv")

# Hình 2.4
plt.figure()
plt.plot(df["gen"], df["best"], color="red")
plt.title("Best Fitness Qua Các Thế Hệ (Elitism)")
plt.xlabel("Thế hệ"); plt.ylabel("Fitness")
plt.savefig("hinh_2_4.png", dpi=150)

# Hình 2.5
fig, ax1 = plt.subplots()
ax2 = ax1.twinx()
ax1.plot(df["gen"], df["best"], color="red", label="best")
ax1.plot(df["gen"], df["avg"], color="orange", label="avg")
ax2.plot(df["gen"], df["std_dev"], color="blue", label="std_dev")
ax1.legend(loc="center left"); ax2.legend(loc="center right")
plt.savefig("hinh_2_5.png", dpi=150)