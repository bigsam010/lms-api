using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmeLms.Models;
namespace SmeLms
{
    public partial class smelmsContext : DbContext
    {
        public smelmsContext() { }

        public smelmsContext(DbContextOptions<smelmsContext> options) : base(options) { }

        public virtual DbSet<Beneficiary> Beneficiary { get; set; }
        public virtual DbSet<Blogpost> Blogpost { get; set; }
        public virtual DbSet<Blogpostimages> Blogpostimages { get; set; }
        public virtual DbSet<Classprogress> Classprogress { get; set; }
        public virtual DbSet<Course> Course { get; set; }
        public virtual DbSet<Coursecategory> Coursecategory { get; set; }
        public virtual DbSet<Courselesson> Courselesson { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<Customercard> Customercard { get; set; }
        public virtual DbSet<Customerclass> Customerclass { get; set; }
        public virtual DbSet<Customersubscription> Customersubscription { get; set; }
        public virtual DbSet<Inclass> Inclass { get; set; }
        public virtual DbSet<Inclassregistration> Inclassregistration { get; set; }
        public virtual DbSet<Lessontopic> Lessontopic { get; set; }
        public virtual DbSet<Loginentry> Loginentry { get; set; }
        public virtual DbSet<Notifications> Notifications { get; set; }
        public virtual DbSet<Passwordreset> Passwordreset { get; set; }
        public virtual DbSet<Paymentlog> Paymentlog { get; set; }
        public virtual DbSet<Planchangerequest> Planchangerequest { get; set; }
        public virtual DbSet<Subscriptionplan> Subscriptionplan { get; set; }
        public virtual DbSet<Subusage> Subusage { get; set; }
        public virtual DbSet<Topiccontent> Topiccontent { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Wishlist> Wishlist { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Beneficiary>(entity =>
            {
                entity.HasKey(e => e.Email)
                    .HasName("PK__benefici__F3DBC57321B6055D");

                entity.ToTable("beneficiary");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Addedby)
                    .IsRequired()
                    .HasColumnName("addedby")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Dateadded)
                    .HasColumnName("dateadded")
                    .HasColumnType("datetime");

                entity.Property(e => e.Ispriviledge).HasColumnName("ispriviledge");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnName("status")
                    .HasMaxLength(7)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<Blogpost>(entity =>
            {
                entity.HasKey(e => e.Postid)
                    .HasName("PK__blogpost__DD017FD20AD2A005");

                entity.ToTable("blogpost");

                entity.Property(e => e.Postid).HasColumnName("postid");

                entity.Property(e => e.Author)
                    .HasColumnName("author")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Caption)
                    .HasColumnName("caption")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                  .HasColumnName("status")
                  .HasMaxLength(10)
                  .IsUnicode(false);

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .HasColumnType("text");

                entity.Property(e => e.Publisheddate)
                    .HasColumnName("publisheddate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Tag)
                    .HasColumnName("tag")
                    .HasColumnType("text");

                entity.Property(e => e.Views).HasColumnName("views");
                entity.Property(e => e.Shares).HasColumnName("shares");
            });


            modelBuilder.Entity<Blogpostimages>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__blogpost__3213E83F73A96013");

                entity.ToTable("blogpostimages");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Blogpost).HasColumnName("blogpost");

                entity.Property(e => e.Filename)
                    .HasColumnName("filename")
                    .HasMaxLength(50)
                    .IsUnicode(false);




            });


            modelBuilder.Entity<Classprogress>(entity =>
            {
                entity.ToTable("classprogress");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Classid).HasColumnName("classid");

                entity.Property(e => e.Contentcompleted).HasColumnName("contentcompleted");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Coursecode)
                    .HasName("PK__course__4116A4767F60ED59");

                entity.ToTable("course");

                entity.Property(e => e.Coursecode)
                    .HasColumnName("coursecode")
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasColumnName("author")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Catid)
                    .IsRequired()
                    .HasColumnName("catid")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Duration)
                    .HasColumnName("duration")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.Enforcesequence).HasColumnName("enforcesequence");

                entity.Property(e => e.Freeonsubcription).HasColumnName("freeonsubcription");
                entity.Property(e => e.Showstudentcount).HasColumnName("showstudentcount");

                entity.Property(e => e.Loyaltypoint).HasColumnName("loyaltypoint");

                entity.Property(e => e.Objectives)
                    .HasColumnName("objectives")
                    .HasColumnType("text");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(8, 2)");

                entity.Property(e => e.Publisheddate)
                    .HasColumnName("publisheddate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Relatedcourses)
                    .HasColumnName("relatedcourses")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.Tag)
                    .HasColumnName("tag")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(100)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<Coursecategory>(entity =>
            {
                entity.HasKey(e => e.Catid)
                    .HasName("PK__courseca__17B9D93E07020F21");

                entity.ToTable("coursecategory");

                entity.Property(e => e.Catid).HasColumnName("catid");

                entity.Property(e => e.Datecreated)
                    .HasColumnName("datecreated")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Courselesson>(entity =>
            {
                entity.HasKey(e => e.Lessonid)
                    .HasName("PK__coursele__90213948412EB0B6");

                entity.ToTable("courselesson");

                entity.Property(e => e.Lessonid).HasColumnName("lessonid");

                entity.Property(e => e.Coursecode)
                    .IsRequired()
                    .HasColumnName("coursecode")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Position)
                 .HasColumnName("position");


            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Email)
                    .HasName("pk_customer");

                entity.ToTable("customer");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Accounttype)
                    .HasColumnName("accounttype")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Companyname)
                    .HasColumnName("companyname")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Firstname)
                    .HasColumnName("firstname")
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Isverified).HasColumnName("isverified");

                entity.Property(e => e.Joindate)
                    .HasColumnName("joindate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Lastname)
                    .HasColumnName("lastname")
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Loyalitypoint).HasColumnName("loyalitypoint");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasColumnType("text");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnName("status")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.Verificationtoken)
                    .HasColumnName("verificationtoken")
                    .HasColumnType("text");

                entity.Property(e => e.Accountcategory)
                    .HasColumnName("accountcategory")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Customercard>(entity =>
            {
                entity.HasKey(e => e.Cardnumber)
                    .HasName("pk_cus");

                entity.ToTable("customercard");

                entity.Property(e => e.Cardnumber)
                    .HasColumnName("cardnumber")
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Customer)
                    .IsRequired()
                    .HasColumnName("customer")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Cvv)
                    .IsRequired()
                    .HasColumnName("cvv")
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.Dateadded)
                    .HasColumnName("dateadded")
                    .HasColumnType("datetime");

                entity.Property(e => e.Expdate)
                    .IsRequired()
                    .HasColumnName("expdate")
                    .HasMaxLength(5)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<Customerclass>(entity =>
            {
                entity.HasKey(e => e.Classid)
                    .HasName("PK__customer__757438065165187F");

                entity.ToTable("customerclass");

                entity.Property(e => e.Classid).HasColumnName("classid");

                entity.Property(e => e.Coursecode)
                    .IsRequired()
                    .HasColumnName("coursecode")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Customer)
                    .IsRequired()
                    .HasColumnName("customer")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Enddate)
                    .HasColumnName("enddate")
                    .HasColumnType("date");

                entity.Property(e => e.Startdate)
                    .HasColumnName("startdate")
                    .HasColumnType("date");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnName("status")
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(10)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<Customersubscription>(entity =>
            {
                entity.ToTable("customersubscription");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Autorenew).HasColumnName("autorenew");

                entity.Property(e => e.Customer)
                    .HasColumnName("customer")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Expdate)
                    .HasColumnName("expdate")
                    .HasColumnType("date");

                entity.Property(e => e.Paymentref)
                    .HasColumnName("paymentref")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.Subdate)
                    .HasColumnName("subdate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Subid).HasColumnName("subid");

            });

            modelBuilder.Entity<Inclass>(entity =>
            {
                entity.HasKey(e => e.Classid)
                    .HasName("PK__inclass__757438061A14E395");

                entity.ToTable("inclass");

                entity.Property(e => e.Classid).HasColumnName("classid");

                entity.Property(e => e.Catid)
                    .IsRequired()
                    .HasColumnName("catid")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Coursedescription)
                    .IsRequired()
                    .HasColumnName("coursedescription")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Duration)
                    .HasColumnName("duration")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.Enddate)
                    .HasColumnName("enddate")
                    .HasColumnType("date");

                entity.Property(e => e.Loyalitypoint).HasColumnName("loyalitypoint");

                entity.Property(e => e.Objectives)
                    .HasColumnName("objectives")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Startdate)
                    .HasColumnName("startdate")
                    .HasColumnType("date");

                entity.Property(e => e.Starttime)
                    .HasColumnName("starttime")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Timedescription)
                    .HasColumnName("timedescription")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Createdby)
                    .IsRequired()
                    .HasColumnName("createdby")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Price)
                .HasColumnName("price")
                .HasColumnType("decimal(8,2)");

                entity.Property(e => e.Location)
                  .HasColumnName("location")
                  .HasMaxLength(150)
                  .IsUnicode(false);

            });

            modelBuilder.Entity<Inclassregistration>(entity =>
            {
                entity.HasKey(e => e.Regid)
                    .HasName("PK__inclassr__184A6B04164452B1");

                entity.ToTable("inclassregistration");

                entity.Property(e => e.Regid)
                    .HasColumnName("regid")
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Classid).HasColumnName("classid");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fullname)
                    .IsRequired()
                    .HasColumnName("fullname")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Invitedby)
                    .HasColumnName("invitedby")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Regdate)
                    .HasColumnName("regdate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(10)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<Lessontopic>(entity =>
            {
                entity.HasKey(e => e.Topicid)
                    .HasName("PK__lessonto__7C3F755960A75C0F");

                entity.ToTable("lessontopic");

                entity.Property(e => e.Topicid).HasColumnName("topicid");

                entity.Property(e => e.Lessonid).HasColumnName("lessonid");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Position)
                   .IsRequired()
                   .HasColumnName("position");


            });

            modelBuilder.Entity<Loginentry>(entity =>
            {
                entity.ToTable("loginentry");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Client)
                    .IsRequired()
                    .HasColumnName("client")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Logindate)
                    .HasColumnName("logindate")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Notifications>(entity =>
            {
                entity.ToTable("notifications");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Target)
                    .IsRequired()
                    .HasColumnName("target")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnName("remark")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Viewed).HasColumnName("viewed");
                entity.Property(e => e.Notedate)
                    .HasColumnName("notedate")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Passwordreset>(entity =>
            {
                entity.ToTable("passwordreset");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Customer)
                    .HasColumnName("customer")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Expdate)
                    .HasColumnName("expdate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .HasColumnType("text");

            });

            modelBuilder.Entity<Paymentlog>(entity =>
            {
                entity.HasKey(e => e.Refno)
                    .HasName("PK__paymentl__2D28EFC92A4B4B5E");

                entity.ToTable("paymentlog");

                entity.Property(e => e.Refno)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Cardnumber)
                    .HasColumnName("cardnumber")
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Cashamount)
                    .HasColumnName("cashamount")
                    .HasColumnType("decimal(8, 2)");

                entity.Property(e => e.Customer)
                    .HasColumnName("customer")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Loyalitypoints).HasColumnName("loyalitypoints");

                entity.Property(e => e.Paymentdate)
                    .HasColumnName("paymentdate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Paymentmode)
                    .HasColumnName("paymentmode")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Itemref)
                    .HasColumnName("itemref")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Itemdescription)
                 .HasColumnName("itemdescription")
                 .HasMaxLength(150)
                 .IsUnicode(false);


            });

            modelBuilder.Entity<Planchangerequest>(entity =>
            {
                entity.ToTable("planchangerequest");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Customer)
                    .HasColumnName("customer")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Datechanged)
                    .HasColumnName("datechanged")
                    .HasColumnType("datetime");

                entity.Property(e => e.Newplan).HasColumnName("newplan");

                entity.Property(e => e.Oldplan).HasColumnName("oldplan");

                entity.Property(e => e.Paymentref)
                    .IsRequired()
                    .HasColumnName("paymentref")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark)
                    .HasColumnName("remark")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnName("status")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(11)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<Subscriptionplan>(entity =>
            {
                entity.HasKey(e => e.Subid)
                    .HasName("PK__subscrip__B0F1D5B331EC6D26");

                entity.ToTable("subscriptionplan");

                entity.Property(e => e.Subid).HasColumnName("subid");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(7, 2)");

                entity.Property(e => e.Beneficiarycount).HasColumnName("beneficiarycount");

                entity.Property(e => e.Classcount).HasColumnName("classcount");

                entity.Property(e => e.Coursecount).HasColumnName("coursecount");

                entity.Property(e => e.Cycle)
                    .IsRequired()
                    .HasColumnName("cycle")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(12)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Subusage>(entity =>
            {
                entity.ToTable("subusage");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Classtotal).HasColumnName("classtotal");

                entity.Property(e => e.Coursetotal).HasColumnName("coursetotal");

                entity.Property(e => e.Subid).HasColumnName("subid");

            });

            modelBuilder.Entity<Topiccontent>(entity =>
            {
                entity.HasKey(e => e.Contentid)
                    .HasName("PK__topiccon__0BDD8B313D5E1FD2");

                entity.ToTable("topiccontent");

                entity.Property(e => e.Contentid).HasColumnName("contentid");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnName("content")
                    .HasColumnType("text");

                entity.Property(e => e.Contentposition).HasColumnName("contentposition");

                entity.Property(e => e.Contenttype)
                    .IsRequired()
                    .HasColumnName("contenttype")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Fileformat)
                    .HasColumnName("fileformat")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Topicid).HasColumnName("topicid");

            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.Email)
                    .HasName("pk_user");

                entity.ToTable("users");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Dateadded)
                    .HasColumnName("dateadded")
                    .HasColumnType("datetime");

                entity.Property(e => e.Firstname)
                    .HasColumnName("firstname")
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Isprivileged).HasColumnName("isprivileged");

                entity.Property(e => e.Lastname)
                    .HasColumnName("lastname")
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasColumnType("text");

                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.Lastlogin)
                  .HasColumnName("lastlogin")
                  .HasColumnType("datetime");
            });

            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.HasKey(e => e.Wishid)
                    .HasName("PK__wishlist__D2E049D125869641");

                entity.ToTable("wishlist");

                entity.Property(e => e.Wishid).HasColumnName("wishid");

                entity.Property(e => e.Coursecode)
                    .HasColumnName("coursecode")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Customer)
                    .HasColumnName("customer")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Dateadded)
                    .HasColumnName("dateadded")
                    .HasColumnType("datetime");

            });
        }
    }
}